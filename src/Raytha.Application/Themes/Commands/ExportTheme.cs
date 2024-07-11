using FluentValidation;
using MediatR;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.MediaItems;
using Raytha.Application.Themes.WebTemplates;

namespace Raytha.Application.Themes.Commands;

public class ExportTheme
{
    public record Command : LoggableRequest<CommandResponseDto<ThemeJson>>
    {
        public required string DeveloperName { get; init; }

        public static Command Empty() => new()
        {
            DeveloperName = string.Empty,
        };
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.DeveloperName).NotEmpty();
            RuleFor(x => x).Custom((request, _) =>
            {
                if (!db.Themes.Any(t => t.DeveloperName == request.DeveloperName))
                    throw new NotFoundException("Theme", request.DeveloperName);
            });
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ThemeJson>>
    {
        private readonly IRaythaDbContext _db;
        private readonly IFileStorageProvider _fileStorageProvider;

        public Handler(IRaythaDbContext db, IFileStorageProvider fileStorageProvider)
        {
            _db = db;
            _fileStorageProvider = fileStorageProvider;
        }

        public async Task<CommandResponseDto<ThemeJson>> Handle(Command request, CancellationToken cancellationToken)
        {
            var theme = await _db.Themes
                .Where(t => t.DeveloperName == request.DeveloperName)
                .Include(t => t.WebTemplates)
                .Include(t => t.ThemeAccessToMediaItems)
                    .ThenInclude(tm => tm.MediaItem)
                .FirstAsync(cancellationToken);

            var mediaItemsJson = new List<MediaItemsJson>();

            var themeMediaItems = theme.ThemeAccessToMediaItems.Select(tm => tm.MediaItem);
            foreach (var themeMediaItem in themeMediaItems)
            {
                mediaItemsJson.Add(new MediaItemsJson
                {
                    FileName = themeMediaItem!.FileName,
                    DownloadUrl = await _fileStorageProvider.GetDownloadUrlAsync(themeMediaItem.ObjectKey),
                    IsPreviewImage = theme.PreviewImageId == themeMediaItem.Id,
                });
            }

            var themePackage = new ThemeJson
            {
                Title = theme.Title,
                DeveloperName = theme.DeveloperName,
                Description = theme.Description,
                WebTemplates = theme.WebTemplates.Select(WebTemplateJson.GetProjection),
                MediaItems = mediaItemsJson,
            };

            return new CommandResponseDto<ThemeJson>(themePackage);
        }
    }
}