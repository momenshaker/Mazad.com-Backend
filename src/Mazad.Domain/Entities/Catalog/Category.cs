using Mazad.Domain.Common;
using Mazad.Domain.Entities.Listings;

namespace Mazad.Domain.Entities.Catalog;

public class Category : AuditableEntity
{
    public Guid? ParentId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AttributesSchema { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
