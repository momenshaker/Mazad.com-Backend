using Mazad.Application.Common.Models;
using Mazad.Domain.Entities.Bids;

namespace Mazad.Application.Common.Mappings;

public static class BidMappings
{
    public static BidDto ToDto(this Bid bid, Guid viewerId, bool canViewBidder)
    {
        return new BidDto(
            bid.Id,
            bid.Amount,
            bid.PlacedAt,
            bid.Status,
            canViewBidder ? bid.BidderId : null,
            bid.BidderId == viewerId);
    }
}
