using CSharpVitamins;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Utils;
using Raytha.Domain.Entities;
using Raytha.Domain.Events;

namespace Raytha.Application.Themes.EventHandlers;

public class ThemeCreatedEventHandler : INotificationHandler<ThemeCreatedEvent>
{
    private readonly IRaythaDbContext _db;
    private readonly IFileStorageProvider _fileStorageProvider;

    public ThemeCreatedEventHandler(IRaythaDbContext db, IFileStorageProvider fileStorageProvider)
    {
        _db = db;
        _fileStorageProvider = fileStorageProvider;
    }

    public async Task Handle(ThemeCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.InsertDefaultMediaItems)
            await InsertDefaultMediaItemsAsync(notification.ThemeId, cancellationToken);

        await InsertDefaultWebTemplates(notification.ThemeId, notification.InsertDefaultMediaItems, cancellationToken);
    }

    private async Task InsertDefaultMediaItemsAsync(Guid themeId, CancellationToken cancellationToken)
    {
        var mediaItems = new List<MediaItem>();
        var themeAccessToMediaItems = new List<ThemeAccessToMediaItem>();

        var defaultThemeAssetsPath = Path.Combine("wwwroot", "raytha_default_2024", "assets");
        if (!Directory.Exists(defaultThemeAssetsPath))
            throw new DirectoryNotFoundException($"Path '{defaultThemeAssetsPath}' does not exist.");

        var themeFiles = Directory.GetFiles(defaultThemeAssetsPath, "*", SearchOption.AllDirectories);
        foreach (var file in themeFiles)
        {
            var idForKey = ShortGuid.NewGuid();
            var fileName = Path.GetFileName(file);
            var data = await File.ReadAllBytesAsync(file, cancellationToken);
            var objectKey = FileStorageUtility.CreateObjectKeyFromIdAndFileName(idForKey, fileName);
            var contentType = FileStorageUtility.GetMimeType(fileName);

            await _fileStorageProvider.SaveAndGetDownloadUrlAsync(data, objectKey, fileName, contentType, FileStorageUtility.GetDefaultExpiry());

            var mediaItem = new MediaItem
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                FileStorageProvider = _fileStorageProvider.GetName(),
                ObjectKey = objectKey,
                Length = data.Length,
                ContentType = contentType,
            };

            mediaItems.Add(mediaItem);

            themeAccessToMediaItems.Add(new ThemeAccessToMediaItem
            {
                Id = Guid.NewGuid(),
                ThemeId = themeId,
                MediaItemId = mediaItem.Id,
            });
        }

        _db.MediaItems.AddRange(mediaItems);
        _db.ThemeAccessToMediaItems.AddRange(themeAccessToMediaItems);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task InsertDefaultWebTemplates(Guid themeId, bool insertDefaultMediaItems, CancellationToken cancellationToken)
    {
        var loginWebTemplates = new List<string>
        {
            BuiltInWebTemplate.LoginWithEmailAndPasswordPage,
            BuiltInWebTemplate.LoginWithMagicLinkPage,
            BuiltInWebTemplate.LoginWithMagicLinkSentPage,
            BuiltInWebTemplate.ForgotPasswordPage,
            BuiltInWebTemplate.ForgotPasswordCompletePage,
            BuiltInWebTemplate.ForgotPasswordResetLinkSentPage,
            BuiltInWebTemplate.ForgotPasswordSuccessPage,
            BuiltInWebTemplate.UserRegistrationForm,
            BuiltInWebTemplate.UserRegistrationFormSuccess,
            BuiltInWebTemplate.ChangePasswordPage,
            BuiltInWebTemplate.ChangeProfilePage
        };

        var standardWebTemplatesForContentTypes = new List<string>
        {
            BuiltInWebTemplate.HomePage,
            BuiltInWebTemplate.ContentItemDetailViewPage,
            BuiltInWebTemplate.ContentItemListViewPage
        };

        var defaultWebTemplates = new List<WebTemplate>();
        var defaultBaseLayout = BuiltInWebTemplate._Layout;
        var defaultBaseContent = defaultBaseLayout.DefaultContent;

        if (insertDefaultMediaItems)
        {
            var baseLayoutMediaItems = new[]
            {
                "favicon.ico",
                "bootstrap_min.css",
                "bootstrap_bundle_min.js",
            };

            var mediaItemObjectKeys = await _db.ThemeAccessToMediaItems
                .Where(tmi => tmi.ThemeId == themeId)
                .Select(tmi => tmi.MediaItem)
                .Select(mi => mi!.ObjectKey)
                .ToListAsync(cancellationToken);

            foreach (var mediaItemObjectKey in mediaItemObjectKeys)
            {
                var fileName = baseLayoutMediaItems.FirstOrDefault(mi => mediaItemObjectKey.Contains(mi));
                if (fileName != null)
                {
                    defaultBaseContent = defaultBaseContent.Replace(fileName, mediaItemObjectKey);
                }
            }
        }

        var baseLayout = new WebTemplate
        {
            Id = Guid.NewGuid(),
            ThemeId = themeId,
            IsBaseLayout = true,
            IsBuiltInTemplate = true,
            Content = defaultBaseContent,
            Label = defaultBaseLayout.DefaultLabel,
            DeveloperName = defaultBaseLayout.DeveloperName,
        };

        defaultWebTemplates.Add(baseLayout);

        var defaultBaseLoginLayout = BuiltInWebTemplate._LoginLayout;
        var baseLoginLayout = new WebTemplate
        {
            Id = Guid.NewGuid(),
            ThemeId = themeId,
            IsBaseLayout = true,
            IsBuiltInTemplate = true,
            Content = defaultBaseLoginLayout.DefaultContent,
            Label = defaultBaseLoginLayout.DefaultLabel,
            DeveloperName = defaultBaseLoginLayout.DeveloperName,
            ParentTemplateId = baseLayout.Id
        };

        defaultWebTemplates.Add(baseLoginLayout);

        var builtInWebTemplatesWithoutBaseLayout = BuiltInWebTemplate.Templates.Where(bwt =>
            bwt.DeveloperName != BuiltInWebTemplate._Layout.DeveloperName &&
            bwt.DeveloperName != BuiltInWebTemplate._LoginLayout.DeveloperName);

        var contentTypeIds = await _db.ContentTypes
            .Select(ct => ct.Id)
            .ToListAsync(cancellationToken);

        foreach (var webTemplateToBuild in builtInWebTemplatesWithoutBaseLayout)
        {
            var webTemplate = new WebTemplate
            {
                Id = new Guid(),
                ThemeId = themeId,
                ParentTemplateId = baseLayout.Id,
                IsBaseLayout = false,
                IsBuiltInTemplate = true,
                Label = webTemplateToBuild.DefaultLabel,
                DeveloperName = webTemplateToBuild.DeveloperName,
                Content = webTemplateToBuild.DefaultContent,
            };

            if (standardWebTemplatesForContentTypes.Contains(webTemplateToBuild))
            {
                webTemplate.IsBuiltInTemplate = false;
                webTemplate.AllowAccessForNewContentTypes = true;
                webTemplate.TemplateAccessToModelDefinitions = contentTypeIds
                    .Select(contentTypeId => new WebTemplateAccessToModelDefinition { ContentTypeId = contentTypeId })
                    .ToList();

                if (webTemplate.DeveloperName == BuiltInWebTemplate.HomePage.DeveloperName)
                {
                    const string fileName = "raythadotcom_screenshot.webp";

                    var mediaItemObjectKey = await _db.ThemeAccessToMediaItems
                        .Where(tmi => tmi.ThemeId == themeId)
                        .Select(tmi => tmi.MediaItem)
                        .Where(mi => mi!.FileName.Contains(fileName))
                        .Select(mi => mi!.ObjectKey)
                        .FirstAsync(cancellationToken);

                    webTemplate.Content = webTemplate.Content.Replace(fileName, mediaItemObjectKey);
                }
            }
            else if (loginWebTemplates.Contains(webTemplateToBuild))
            {
                webTemplate.ParentTemplateId = baseLoginLayout.Id;
            }

            defaultWebTemplates.Add(webTemplate);
        }

        _db.WebTemplates.AddRange(defaultWebTemplates);
        await _db.SaveChangesAsync(cancellationToken);
    }
}