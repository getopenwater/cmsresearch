using CSharpVitamins;
using MediatR;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.WebTemplates.Queries;

public class GetWebTemplatesWithoutMappings
{
    public record Query : IRequest<IQueryResponseDto<IReadOnlyCollection<WebTemplateDto>>>
    {
        public required ShortGuid ThemeId { get; init; }
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<IReadOnlyCollection<WebTemplateDto>>>
    {
        private readonly IRaythaDbContext _db;

        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<IReadOnlyCollection<WebTemplateDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var webTemplatesWithoutMapping = new List<WebTemplate>();

            var activeThemeId = await _db.OrganizationSettings.Select(os => os.ActiveThemeId).FirstAsync(cancellationToken);

            var contentItemIds = await _db.ContentItems
                .Select(ci => ci.Id)
                .ToListAsync(cancellationToken);

            foreach (var contentItemId in contentItemIds)
            {
                var anyContentItemMapping = await _db.ThemeWebTemplateContentItemMappings
                    .Where(wtm => wtm.ThemeId == request.ThemeId.Guid)
                    .AnyAsync(wtm => wtm.ContentItemId == contentItemId, cancellationToken);

                if (!anyContentItemMapping)
                {
                    var webTemplate = await _db.ThemeWebTemplateContentItemMappings
                        .Where(wtm => wtm.ThemeId == activeThemeId)
                        .Where(wtm => wtm.ContentItemId == contentItemId)
                        .Select(wtm => wtm.WebTemplate)
                        .FirstAsync(cancellationToken);

                    webTemplatesWithoutMapping.Add(webTemplate!);
                }
            }

            var viewIds = await _db.Views
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);

            foreach (var viewId in viewIds)
            {
                var anyWebTemplateViewMapping = await _db.ThemeWebTemplateViewMappings
                    .Where(wtm => wtm.ThemeId == request.ThemeId.Guid)
                    .AnyAsync(wtm => wtm.ViewId == viewId, cancellationToken);

                if (!anyWebTemplateViewMapping)
                {
                    var webTemplate = await _db.ThemeWebTemplateViewMappings
                        .Where(wtm => wtm.ThemeId == activeThemeId)
                        .Where(wtm => wtm.ViewId == viewId)
                        .Select(wtm => wtm.WebTemplate)
                        .FirstAsync(cancellationToken);

                    webTemplatesWithoutMapping.Add(webTemplate!);
                }
            }

            return new QueryResponseDto<IReadOnlyCollection<WebTemplateDto>>(webTemplatesWithoutMapping.Distinct().Select(WebTemplateDto.GetProjection).ToList()!);
        }
    }
}