using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;

namespace Raytha.Application.Themes.WebTemplates.Queries;

public class GetActiveThemeWebTemplateByDeveloperName
{
    public record Query : IRequest<IQueryResponseDto<WebTemplateDto>>
    {
        public required string DeveloperName { get; init; }
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<WebTemplateDto>>
    {
        private readonly IRaythaDbContext _db;

        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<WebTemplateDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var activeThemeId = await _db.OrganizationSettings
                .Select(os => os.ActiveThemeId)
                .FirstAsync(cancellationToken);

            var entity = _db.WebTemplates
                .Where(wt => wt.ThemeId == activeThemeId)
                .Include(p => p.TemplateAccessToModelDefinitions)
                    .ThenInclude(p => p.ContentType)
                .FirstOrDefault(p => p.DeveloperName == request.DeveloperName.ToDeveloperName());

            if (entity == null)
                throw new NotFoundException("Template", request.DeveloperName);

            if (entity.ParentTemplateId != null)
                await WebTemplateUtility.LoadParentWebTemplatesRecursiveAsync(entity, _db, cancellationToken);

            return new QueryResponseDto<WebTemplateDto>(WebTemplateDto.GetProjection(entity)!);
        }
    }
}
