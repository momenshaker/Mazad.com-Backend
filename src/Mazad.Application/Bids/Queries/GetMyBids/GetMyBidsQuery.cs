using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Bids.Queries.GetMyBids;

public record GetMyBidsQuery(Guid BidderId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<BidDto>>;

public class GetMyBidsQueryHandler : IRequestHandler<GetMyBidsQuery, PagedResult<BidDto>>
{
    private readonly IMazadDbContext _context;

    public GetMyBidsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BidDto>> Handle(GetMyBidsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bids
            .AsNoTracking()
            .Where(b => b.BidderId == request.BidderId)
            .OrderByDescending(b => b.PlacedAt);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items
            .Select(b => b.ToDto(request.BidderId, canViewBidder: true))
            .ToList();

        return new PagedResult<BidDto>(dtos, request.Page, request.PageSize, total);
    }
}
