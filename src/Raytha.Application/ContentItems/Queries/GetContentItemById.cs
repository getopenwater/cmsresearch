using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;

namespace Raytha.Application.ContentItems.Queries;

public class GetContentItemById
{
    public record Query : GetEntityByIdInputDto, IRequest<IQueryResponseDto<ContentItemDto>>
    {
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ContentItemDto>>
    {
        private readonly IRaythaDbJsonQueryEngine _db;
        private readonly IContentTypeInRoutePath _contentTypeInRoutePath;
        private readonly IRaythaDbContext _entityFrameworkDb;

        public Handler(IRaythaDbJsonQueryEngine db, IContentTypeInRoutePath contentTypeInRoutePath, IRaythaDbContext entityFrameworkDb)
        {
            _db = db;
            _contentTypeInRoutePath = contentTypeInRoutePath;
            _entityFrameworkDb = entityFrameworkDb;
        }
        
        public async Task<IQueryResponseDto<ContentItemDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = _db
                .FirstOrDefault(request.Id.Guid);

            var activeThemeId = await _entityFrameworkDb.OrganizationSettings
                .Select(os => os.ActiveThemeId)
                .FirstAsync(cancellationToken);

            var webTemplate = await _entityFrameworkDb.ThemeWebTemplateContentItemMappings
                .Where(wtm => wtm.ThemeId == activeThemeId)
                .Where(wtm => wtm.ContentItemId == entity.Id)
                .Select(wtm => wtm.WebTemplate)
                .FirstAsync(cancellationToken);

            if (entity == null)
                throw new NotFoundException("Content item", request.Id);

            _contentTypeInRoutePath.ValidateContentTypeInRoutePathMatchesValue(entity.ContentType.DeveloperName);

            return new QueryResponseDto<ContentItemDto>(ContentItemDto.GetProjection(entity, webTemplate!));
        }
    }
}
