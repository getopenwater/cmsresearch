﻿using System.Text.Json;
using CSharpVitamins;
using FluentValidation;
using MediatR;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.Commands;

public class BeginImportThemeFromUrl
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public required string Url { get; init; }

        public static Command Empty() => new()
        {
            Url = string.Empty,
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Url).NotEmpty();
            RuleFor(x => x).Custom((request, context) =>
            {
                if (!request.Url.IsValidUriFormat())
                    context.AddFailure("Url", $"Invalid url format: {request.Url}");
            });
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IBackgroundTaskQueue _taskQueue;

        public Handler(IBackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
        }

        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var backgroundJobId = await _taskQueue.EnqueueAsync<BackgroundTask>(request, cancellationToken);

            return new CommandResponseDto<ShortGuid>(backgroundJobId);
        }
    }

    public class BackgroundTask : IExecuteBackgroundTask
    {
        private readonly IRaythaDbContext _db;
        private readonly IFileStorageProvider _fileStorageProvider;

        private static readonly HttpClient _httpClient = new HttpClient();

        public BackgroundTask(IRaythaDbContext db, IFileStorageProvider fileStorageProvider)
        {
            _db = db;
            _fileStorageProvider = fileStorageProvider;
        }

        public async Task Execute(Guid jobId, JsonElement args, CancellationToken cancellationToken)
        {
            // download json and deserialize
            var job = _db.BackgroundTasks.First(p => p.Id == jobId);

            job.TaskStep = 1;
            job.StatusInfo = "Beginning import. Downloading theme json package from the url.";
            job.PercentComplete = 20;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);

            var urlToDownloadJson = args.GetProperty("Url").GetString();
            var themePackageJson = await GetJsonFromUrl(urlToDownloadJson!, cancellationToken);
            var themePackage = JsonSerializer.Deserialize<ThemeJson>(themePackageJson);

            job.TaskStep = 2;
            job.StatusInfo = $"Importing theme - {themePackage!.Title}";
            job.PercentComplete = 40;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);

            if (_db.Themes.Any(t => t.DeveloperName == themePackage.DeveloperName))
            {
                job.ErrorMessage = $"Failed to import. Theme with developer name {themePackage.DeveloperName} is already exists";
                job.Status = BackgroundTaskStatus.Error;
                _db.BackgroundTasks.Update(job);

                await _db.SaveChangesAsync(cancellationToken);

                Thread.Sleep(1500);

                return;
            }

            var themeId = Guid.NewGuid();
            var theme = new Theme
            {
                Id = themeId,
                Title = themePackage.Title,
                DeveloperName = themePackage.DeveloperName,
                Description = themePackage.Description,
            };

            _db.Themes.Add(theme);

            // importing web-templates
            job.TaskStep = 3;
            job.StatusInfo = "Importing web-templates";
            job.PercentComplete = 60;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);

            var webTemplates = themePackage.WebTemplates.Select(webTemplateFromThemePackage => new WebTemplate
            {
                Id = Guid.NewGuid(),
                ThemeId = themeId,
                Label = webTemplateFromThemePackage.Label,
                DeveloperName = webTemplateFromThemePackage.DeveloperName,
                Content = webTemplateFromThemePackage.Content,
                ParentTemplateId = webTemplateFromThemePackage.ParentTemplateId,
                IsBaseLayout = webTemplateFromThemePackage.IsBaseLayout,
                AllowAccessForNewContentTypes = webTemplateFromThemePackage.AllowAccessForNewContentTypes,
                IsBuiltInTemplate = webTemplateFromThemePackage.IsBuiltInTemplate,
            }).ToList();

            _db.WebTemplates.AddRange(webTemplates);

            // importing media items and download files
            var mediaItems = new List<MediaItem>();
            var themeAccessToMediaItems = new List<ThemeAccessToMediaItem>();

            job.TaskStep = 4;
            job.StatusInfo = "Importing media items and downloading files";
            job.PercentComplete = 80;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var mediaItemInThemePackage in themePackage.MediaItems)
            {
                var contentType = FileStorageUtility.GetMimeType(mediaItemInThemePackage.FileName);
                var file = await GetDataFromUrl(mediaItemInThemePackage.DownloadUrl, cancellationToken);
                var objectKey = FileStorageUtility.CreateObjectKeyFromIdAndFileName(theme.DeveloperName, mediaItemInThemePackage.FileName);

                await _fileStorageProvider.SaveAndGetDownloadUrlAsync(file, objectKey, mediaItemInThemePackage.FileName, contentType, DateTime.MaxValue);

                var mediaItem = new MediaItem
                {
                    Id = Guid.NewGuid(),
                    FileName = mediaItemInThemePackage.FileName,
                    Length = file.Length,
                    ContentType = contentType,
                    ObjectKey = objectKey,
                    FileStorageProvider = _fileStorageProvider.GetName(),
                };

                themeAccessToMediaItems.Add(new ThemeAccessToMediaItem
                {
                    Id = Guid.NewGuid(),
                    MediaItemId = mediaItem.Id,
                    ThemeId = theme.Id,
                });

                if (mediaItemInThemePackage.IsPreviewImage)
                {
                    theme.PreviewImageId = mediaItem.Id;
                }

                mediaItems.Add(mediaItem);
            }

            await _db.MediaItems.AddRangeAsync(mediaItems, cancellationToken);
            await _db.ThemeAccessToMediaItems.AddRangeAsync(themeAccessToMediaItems, cancellationToken);

            job.TaskStep = 5;
            job.StatusInfo = "Finished importing.";
            job.PercentComplete = 100;
            _db.BackgroundTasks.Update(job);

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GetJsonFromUrl(string url, CancellationToken cancellationToken)
        {
            var content = await GetContentByUrl(url, cancellationToken);

            return await content.ReadAsStringAsync(cancellationToken);
        }

        private async Task<byte[]> GetDataFromUrl(string url, CancellationToken cancellationToken)
        {
            var content = await GetContentByUrl(url, cancellationToken);

            return await content.ReadAsByteArrayAsync(cancellationToken);
        }

        private async Task<HttpContent> GetContentByUrl(string urlToDownload, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(urlToDownload, cancellationToken);

            if (response == null)
                throw new Exception($"Unable to retrieve file from {urlToDownload}. Reason unknown.");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Unable to retrieve file from {urlToDownload}: {response.StatusCode} - {response.ReasonPhrase}");

            return response.Content;
        }
    }
}