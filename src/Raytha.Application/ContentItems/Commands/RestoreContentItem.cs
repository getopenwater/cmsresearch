using CSharpVitamins;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Domain.Entities;

namespace Raytha.Application.ContentItems.Commands;

public class RestoreContentItem
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IRaythaDbContext _db;
        private readonly IContentTypeInRoutePath _contentTypeInRoutePath;
        public Handler(IRaythaDbContext db, IContentTypeInRoutePath contentTypeInRoutePath)
        {
            _db = db;
            _contentTypeInRoutePath = contentTypeInRoutePath;
        }
        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = _db.DeletedContentItems
                .Include(p => p.ContentType)
                .FirstOrDefault(p => p.Id == request.Id.Guid);

            if (entity == null)
                throw new NotFoundException("Content Item", request.Id);

            _contentTypeInRoutePath.ValidateContentTypeInRoutePathMatchesValue(entity.ContentType.DeveloperName);

            var templateId = Guid.Empty;
            var activeThemeId = await _db.OrganizationSettings
                .Select(os => os.ActiveThemeId)
                .FirstAsync(cancellationToken);

            var template = await _db.WebTemplates
                .Where(wt => wt.ThemeId == activeThemeId)
                .FirstOrDefaultAsync(wt => wt.Id == entity.WebTemplateId, cancellationToken);

            if (template != null)
            {
                templateId = template.Id;
            }
            else
            {
                templateId = await _db.WebTemplates
                    .Where(wt => wt.ThemeId == activeThemeId)
                    .Where(wt => wt.DeveloperName == BuiltInWebTemplate.ContentItemDetailViewPage)
                    .Select(wt => wt.Id)
                    .FirstAsync(cancellationToken);
            }

            string path = string.Empty;
            var routePathExists = _db.Routes.FirstOrDefault(p => p.Path.ToLower() == entity.RoutePath);
            if (routePathExists != null)
            {
                path = $"{entity.ContentType.DeveloperName}/{(ShortGuid)entity.Id}";
            }
            else
            {
                path = entity.RoutePath;
            }

            var restoredEntity = new ContentItem
            {
                _PublishedContent = entity._PublishedContent,
                _DraftContent = entity._PublishedContent,
                ContentTypeId = entity.ContentTypeId,
                Id = entity.OriginalContentItemId,
                IsDraft = false,
                IsPublished = false,
                Route = new Route
                {
                    Path = path,
                    ContentItemId = entity.OriginalContentItemId
                }
            };
            _db.ContentItems.Add(restoredEntity);
            _db.DeletedContentItems.Remove(entity);

            var webTemplateContentItemMapping = new ThemeWebTemplateContentItemMapping
            {
                ThemeId = activeThemeId,
                WebTemplateId = templateId,
                ContentItemId = restoredEntity.Id,
            };

            await _db.ThemeWebTemplateContentItemMappings.AddAsync(webTemplateContentItemMapping, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.OriginalContentItemId);
        }
    }
}
