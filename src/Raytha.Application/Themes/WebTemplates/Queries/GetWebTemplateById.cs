using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;

namespace Raytha.Application.Themes.WebTemplates.Queries;

public class GetWebTemplateById
{
    public record Query : GetEntityByIdInputDto, IRequest<IQueryResponseDto<WebTemplateDto>>
    {
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
            var entity = await _db.WebTemplates
                .Include(p => p.TemplateAccessToModelDefinitions)
                    .ThenInclude(p => p.ContentType)
                .FirstOrDefaultAsync(p => p.Id == request.Id.Guid, cancellationToken);

            if (entity == null)
                throw new NotFoundException("Template", request.Id);

            if (entity.ParentTemplateId != null)
                await WebTemplateUtility.LoadParentWebTemplatesRecursiveAsync(entity, _db, cancellationToken);

            return new QueryResponseDto<WebTemplateDto>(WebTemplateDto.GetProjection(entity)!);
        }
    }
}
