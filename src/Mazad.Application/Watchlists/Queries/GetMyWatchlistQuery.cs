using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Watchlists.Queries;

public record GetMyWatchlistQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ListingDto>>;

public class GetMyWatchlistQueryHandler : IRequestHandler<GetMyWatchlistQuery, PagedResult<ListingDto>>
{
    private readonly IMazadDbContext _context;

    public GetMyWatchlistQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ListingDto>> Handle(GetMyWatchlistQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Watchlists
            .AsNoTracking()
            .Where(w => w.UserId == request.UserId)
            .Join(
                _context.Listings.Include(l => l.Media),
                w => w.ListingId,
                l => l.Id,
                (w, l) => l)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(l => l.ToDto()).ToList();
        return new PagedResult<ListingDto>(dtos, request.Page, request.PageSize, total);
    }
}
