using System.Linq;
using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using Mazad.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Bids.Queries.GetListingBids;

public record GetListingBidsQuery(
    Guid ListingId,
    Guid ViewerId,
    bool ViewerIsAdmin,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<BidDto>>;

public class GetListingBidsQueryHandler : IRequestHandler<GetListingBidsQuery, PagedResult<BidDto>>
{
    private readonly IMazadDbContext _context;

    public GetListingBidsQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BidDto>> Handle(GetListingBidsQuery request, CancellationToken cancellationToken)
    {
        var listing = await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            throw new NotFoundException("Listing", request.ListingId);
        }

        var canViewAllBidderInfo = request.ViewerIsAdmin || listing.SellerId == request.ViewerId;

        var query = _context.Bids
            .AsNoTracking()
            .Where(b => b.ListingId == request.ListingId)
            .Where(b => b.Status != BidStatus.Invalid)
            .OrderByDescending(b => b.PlacedAt);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items
            .Select(b => b.ToDto(request.ViewerId, canViewAllBidderInfo || b.BidderId == request.ViewerId))
            .ToList();

        return new PagedResult<BidDto>(dtos, request.Page, request.PageSize, total);
    }
}
