using MediatR;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Raytha.Application.Themes.WebTemplates.Queries;

public class GetLatestWebTemplate
{
    public record Query : IRequest<IQueryResponseDto<IReadOnlyCollection<WebTemplateDto>>>
    {
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
            var latestWebTemplates = await _db.WebTemplates
                .GroupBy(nmr => nmr.ThemeId)
                .Select(g => g.FirstOrDefault(r => r.CreationTime == g.Max(nmr => nmr.CreationTime)))
                .ToArrayAsync(cancellationToken);

            return new QueryResponseDto<IReadOnlyCollection<WebTemplateDto>>(latestWebTemplates
                .Select(WebTemplateDto.GetProjection)
                .ToArray()!);
        }
    }
}