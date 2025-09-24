using Mazad.Domain.Common;
using Mazad.Domain.Entities.Listings;
using Mazad.Domain.Enums;

namespace Mazad.Domain.Entities.Bids;

public class Bid : AuditableEntity
{
    public Guid ListingId { get; set; }
    public Guid BidderId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public BidStatus Status { get; set; } = BidStatus.Placed;

    public Listing? Listing { get; set; }
}
