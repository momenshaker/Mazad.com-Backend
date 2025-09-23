using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Listings;

public class ListingMedia : AuditableEntity
{
    public Guid ListingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCover { get; set; }

    public Listing? Listing { get; set; }
}
