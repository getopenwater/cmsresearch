using CSharpVitamins;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Models;
using Raytha.Application.Common.Utils;
using Raytha.Domain.ValueObjects;

namespace Raytha.Application.Themes.Queries;

public class GetThemeRevisionsByThemeId
{
    public record Query : GetPagedEntitiesInputDto, IRequest<IQueryResponseDto<ListResultDto<ThemeRevisionDto>>>
    {
        public required ShortGuid ThemeId { get; init; }
        public override string OrderBy { get; init; } = $"CreationTime {SortOrder.Descending}";
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ListResultDto<ThemeRevisionDto>>>
    {
        private readonly IRaythaDbContext _db;

        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<ListResultDto<ThemeRevisionDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _db.ThemeRevisions
                .Include(tr => tr.CreatorUser)
                .Where(tr => tr.ThemeId == request.ThemeId.Guid)
                .AsQueryable();

            var total = await query.CountAsync(cancellationToken);
            var items = await query.ApplyPaginationInput(request).Select(ThemeRevisionDto.GetProjection()).ToArrayAsync(cancellationToken);

            return new QueryResponseDto<ListResultDto<ThemeRevisionDto>>(new ListResultDto<ThemeRevisionDto>(items, total));
        }
    }
}