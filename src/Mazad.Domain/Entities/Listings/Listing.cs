using Mazad.Domain.Common;
using Mazad.Domain.Entities.Bids;
using Mazad.Domain.Entities.Catalog;
using Mazad.Domain.Enums;

namespace Mazad.Domain.Entities.Listings;

public class Listing : AuditableEntity
{
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Attributes { get; set; }
    public ListingType Type { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public decimal? StartPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? BidIncrement { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public int Views { get; set; }
    public int WatchCount { get; set; }
    public string? ModerationNotes { get; set; }
    public string? RejectionReason { get; set; }

    public Category? Category { get; set; }
    public ICollection<ListingMedia> Media { get; set; } = new List<ListingMedia>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
