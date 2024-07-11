using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;

namespace Raytha.Application.Themes.Queries;

public class GetLatestThemeRevisions
{
    public record Query : IRequest<IQueryResponseDto<IReadOnlyCollection<ThemeRevisionDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<IReadOnlyCollection<ThemeRevisionDto>>>
    {
        private readonly IRaythaDbContext _db;

        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<IReadOnlyCollection<ThemeRevisionDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var latestThemeRevisions = await _db.ThemeRevisions
                .GroupBy(tr => tr.ThemeId)
                .Select(g => g.FirstOrDefault(r => r.CreationTime == g.Max(tr => tr.CreationTime)))
                .ToArrayAsync(cancellationToken);

            return new QueryResponseDto<IReadOnlyCollection<ThemeRevisionDto>>(latestThemeRevisions
                .Select(ThemeRevisionDto.GetProjection!)
                .ToArray());
        }
    }
}