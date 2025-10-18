using Mazad.Application.Abstractions.Persistence;
using Mazad.Application.Common.Exceptions;
using Mazad.Application.Common.Mappings;
using Mazad.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mazad.Application.Bids.Queries.GetBidById;

public record GetBidByIdQuery(Guid BidId, Guid ViewerId, bool ViewerIsAdmin) : IRequest<BidDto>;

public class GetBidByIdQueryHandler : IRequestHandler<GetBidByIdQuery, BidDto>
{
    private readonly IMazadDbContext _context;

    public GetBidByIdQueryHandler(IMazadDbContext context)
    {
        _context = context;
    }

    public async Task<BidDto> Handle(GetBidByIdQuery request, CancellationToken cancellationToken)
    {
        var bid = await _context.Bids
            .AsNoTracking()
            .Include(b => b.Listing)
            .FirstOrDefaultAsync(b => b.Id == request.BidId, cancellationToken);

        if (bid is null)
        {
            throw new NotFoundException("Bid", request.BidId);
        }

        if (bid.Listing is null)
        {
            throw new NotFoundException("Listing", bid.ListingId);
        }

        var canViewAllBidderInfo = request.ViewerIsAdmin
            || bid.Listing.SellerId == request.ViewerId
            || bid.BidderId == request.ViewerId;

        return bid.ToDto(request.ViewerId, canViewAllBidderInfo);
    }
}
