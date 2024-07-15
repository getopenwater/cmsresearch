using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Utils;
using Raytha.Application.OrganizationSettings.Commands;
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
        var theme = notification.Theme;

        if (!string.IsNullOrEmpty(notification.ImageBase64) && !string.IsNullOrEmpty(notification.ImageFileName) && !string.IsNullOrEmpty(notification.ImageFileType))
        {
            var previewImageMediaItemId = await InsertPreviewImageMediaItem(notification.ImageBase64!, notification.ImageFileName!, notification.ImageFileType!, theme.Id, theme.DeveloperName, cancellationToken);

            theme.PreviewImageId = previewImageMediaItemId;
            _db.Themes.Update(theme);
        }

        await InsertDefaultWebTemplates(theme.Id, theme.DeveloperName, cancellationToken);

        if (notification.InsertDefaultMediaItems)
            await InsertDefaultMediaItemsAsync(theme.Id, theme.DeveloperName, cancellationToken);

        await SaveActiveThemeWebTemplateMappingsIfNotExists(cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> InsertPreviewImageMediaItem(string imageBase64, string imageFileName, string imageFileType, Guid themeId, string themeDeveloperName, CancellationToken cancellationToken)
    {
        var contentType = FileStorageUtility.GetMimeType(imageFileName);
        var previewImageData = Convert.FromBase64String(imageBase64);
        var objectKey = FileStorageUtility.CreateObjectKeyFromIdAndFileName(themeDeveloperName, imageFileName);
        await _fileStorageProvider.SaveAndGetDownloadUrlAsync(previewImageData, objectKey, imageFileName, contentType, DateTime.MaxValue);

        var previewImageMediaItem = new MediaItem
        {
            Id = Guid.NewGuid(),
            ContentType = contentType,
            FileName = imageFileName,
            Length = previewImageData.Length,
            FileStorageProvider = _fileStorageProvider.GetName(),
            ObjectKey = objectKey,
        };

        _db.MediaItems.Add(previewImageMediaItem);

        var themeAccessToMediaItem = new ThemeAccessToMediaItem
        {
            Id = Guid.NewGuid(),
            MediaItemId = previewImageMediaItem.Id,
            ThemeId = themeId,
        };

        await _db.ThemeAccessToMediaItems.AddAsync(themeAccessToMediaItem, cancellationToken);

        return previewImageMediaItem.Id;
    }

    private async Task InsertDefaultWebTemplates(Guid themeId, string themeDeveloperName, CancellationToken cancellationToken)
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
        var updatedContent = defaultBaseLayout.DefaultContent.Replace(Theme.DEFAULT_THEME_DEVELOPER_NAME, themeDeveloperName);
        var baseLayout = new WebTemplate
        {
            Id = Guid.NewGuid(),
            ThemeId = themeId,
            IsBaseLayout = true,
            IsBuiltInTemplate = true,
            Content = updatedContent,
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

        var pagesContentTypeId = await _db.ContentTypes
            .Where(ct => ct.DeveloperName == InitialSetup.Handler.PAGES_DEVELOPER_NAME)
        .Select(ct => ct.Id)
            .FirstAsync(cancellationToken);

        var postsContentTypeId = await _db.ContentTypes
            .Where(ct => ct.DeveloperName == InitialSetup.Handler.POSTS_DEVELOPER_NAME)
            .Select(ct => ct.Id)
            .FirstAsync(cancellationToken);

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
                Content = webTemplateToBuild.DeveloperName == BuiltInWebTemplate.HomePage
                    ? webTemplateToBuild.DefaultContent.Replace(Theme.DEFAULT_THEME_DEVELOPER_NAME, themeDeveloperName)
                    : webTemplateToBuild.DefaultContent,
            };

            if (standardWebTemplatesForContentTypes.Contains(webTemplateToBuild))
            {
                webTemplate.IsBuiltInTemplate = false;
                webTemplate.AllowAccessForNewContentTypes = true;
                webTemplate.TemplateAccessToModelDefinitions = new List<WebTemplateAccessToModelDefinition>
                {
                    new() { ContentTypeId = pagesContentTypeId },
                    new() { ContentTypeId = postsContentTypeId }
                };
            }
            else if (loginWebTemplates.Contains(webTemplateToBuild))
            {
                webTemplate.ParentTemplateId = baseLoginLayout.Id;
            }

            defaultWebTemplates.Add(webTemplate);
        }

        _db.WebTemplates.AddRange(defaultWebTemplates);
    }

    private async Task InsertDefaultMediaItemsAsync(Guid themeId, string themeDeveloperName, CancellationToken cancellationToken)
    {
        var mediaItems = new List<MediaItem>();
        var themeAccessToMediaItems = new List<ThemeAccessToMediaItem>();

        var defaultThemeAssetsPath = Path.Combine("wwwroot", "raytha_default_2024", "assets");
        if (!Directory.Exists(defaultThemeAssetsPath))
            throw new DirectoryNotFoundException($"Path '{defaultThemeAssetsPath}' does not exist.");

        var themeFiles = Directory.GetFiles(defaultThemeAssetsPath, "*", SearchOption.AllDirectories);
        foreach (var file in themeFiles)
        {
            if (file.Contains("raythadotcom_screenshot.webp") || file.Contains(Theme.PREVIEW_IMAGE_FILENAME))
                continue;

            var fileName = Path.GetFileName(file);
            var data = await File.ReadAllBytesAsync(file, cancellationToken);
            var objectKey = FileStorageUtility.CreateObjectKeyFromIdAndFileName(themeDeveloperName, fileName);
            var contentType = FileStorageUtility.GetMimeType(fileName);

            await _fileStorageProvider.SaveAndGetDownloadUrlAsync(data, objectKey, fileName, contentType, DateTime.MaxValue);

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
    }

    private async Task SaveActiveThemeWebTemplateMappingsIfNotExists(CancellationToken cancellationToken)
    {
        var activeThemeId = await _db.Themes
            .Where(t => t.IsActive == true)
            .Select(t => t.Id)
            .FirstAsync(cancellationToken);

        var contentItems = await _db.ContentItems.ToListAsync(cancellationToken);
        foreach (var contentItem in contentItems)
        {
            var webTemplateMappingByContentItemId = await _db.ThemeWebTemplatesMappings
                .Where(wtm => wtm.ContentItemId == contentItem.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (webTemplateMappingByContentItemId == null)
            {
                var webTemplateMapping = new ThemeWebTemplatesMapping
                {
                    Id = Guid.NewGuid(),
                    ThemeId = activeThemeId,
                    WebTemplateId = contentItem.WebTemplateId,
                    ContentItemId = contentItem.Id,
                };

                await _db.ThemeWebTemplatesMappings.AddAsync(webTemplateMapping, cancellationToken);
            }
        }

        var views = await _db.Views.ToListAsync(cancellationToken);
        foreach (var view in views)
        {
            var webTemplateMappingByViewId = await _db.ThemeWebTemplatesMappings
                .Where(wtm => wtm.ViewId == view.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (webTemplateMappingByViewId == null)
            {
                var webTemplateMapping = new ThemeWebTemplatesMapping
                {
                    Id = Guid.NewGuid(),
                    ThemeId = activeThemeId,
                    WebTemplateId = view.WebTemplateId,
                    ViewId = view.Id,
                };

                await _db.ThemeWebTemplatesMappings.AddAsync(webTemplateMapping, cancellationToken);
            }
        }
    }
}