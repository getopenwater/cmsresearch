using CSharpVitamins;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;

namespace Raytha.Application.Themes.Queries;

public class GetAnyWebTemplateMappings
{
    public record Query : IRequest<IQueryResponseDto<ThemeWebTemplateMappingDto>>
    {
        public required ShortGuid ThemeId { get; init; }
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ThemeWebTemplateMappingDto>>
    {
        private readonly IRaythaDbContext _db;

        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<ThemeWebTemplateMappingDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var anyContentItemMappingExists = await _db.ContentItems
                .AllAsync(ci => _db.ThemeWebTemplateContentItemMappings.Any(wtm => wtm.ThemeId == request.ThemeId.Guid && wtm.ContentItemId == ci.Id), cancellationToken);

            var anyViewMappingExists = await _db.Views
                .AllAsync(v => _db.ThemeWebTemplateViewMappings.Any(wtm => wtm.ThemeId == request.ThemeId.Guid && wtm.ViewId == v.Id), cancellationToken);

            return new QueryResponseDto<ThemeWebTemplateMappingDto>(ThemeWebTemplateMappingDto.GetProjection(anyContentItemMappingExists, anyViewMappingExists));
        }
    }
}