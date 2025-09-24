using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Watchlists;

public class WatchlistItem : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ListingId { get; set; }
}
