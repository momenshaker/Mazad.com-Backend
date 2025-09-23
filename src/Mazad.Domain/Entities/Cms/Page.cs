using Mazad.Domain.Common;
using Mazad.Domain.Enums;

namespace Mazad.Domain.Entities.Cms;

public class Page : AuditableEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Blocks { get; set; } = string.Empty;
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoImage { get; set; }
    public PageStatus Status { get; set; } = PageStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
}
