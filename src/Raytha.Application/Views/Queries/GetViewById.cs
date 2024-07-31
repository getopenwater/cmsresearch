using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Exceptions;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;

namespace Raytha.Application.Views.Queries;

public class GetViewById
{
    public record Query : GetEntityByIdInputDto, IRequest<IQueryResponseDto<ViewDto>>
    {
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ViewDto>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<ViewDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = _db.Views
                .Include(p => p.Route)
                .Include(p => p.ContentType)
                .ThenInclude(p => p.ContentTypeFields)
                .FirstOrDefault(p => p.Id == request.Id.Guid);

            if (entity == null)
                throw new NotFoundException("View", request.Id);

            var activeThemeId = await _db.OrganizationSettings
                .Select(os => os.ActiveThemeId)
                .FirstAsync(cancellationToken);

            var webTemplate = await _db.ThemeWebTemplateViewMappings
                .Where(wtm => wtm.ThemeId == activeThemeId)
                .Where(wtm => wtm.ViewId == entity.Id)
                .Select(wtm => wtm.WebTemplate)
                .FirstOrDefaultAsync(cancellationToken);

            if (webTemplate == null)
                throw new NotFoundException("webTemplate", request.Id);

            return new QueryResponseDto<ViewDto>(ViewDto.GetProjection(entity, webTemplate));
        }
    }
}
