using FluentValidation;
using MediatR;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Utils;
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
            RuleFor(x => x).Custom((request, context) =>
            {
                var entity = db.Themes.FirstOrDefault(t => t.DeveloperName == request.DeveloperName.ToDeveloperName());

                if (entity == null)
                    throw new NotFoundException("Theme", request.DeveloperName);

                if (!entity.IsExportable)
                    context.AddFailure("IsExportable", "The theme can not be exported");
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
                    DownloadUrl = await _fileStorageProvider.GetDownloadUrlAsync(themeMediaItem.ObjectKey, FileStorageUtility.GetDefaultExpiry()),
                });
            }

            var themePackage = new ThemeJson
            {
                WebTemplates = theme.WebTemplates.Select(WebTemplateJson.GetProjection),
                MediaItems = mediaItemsJson,
            };

            return new CommandResponseDto<ThemeJson>(themePackage);
        }
    }
}