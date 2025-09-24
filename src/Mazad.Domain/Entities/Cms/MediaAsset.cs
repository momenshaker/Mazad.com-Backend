using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Cms;

public class MediaAsset : AuditableEntity
{
    public string Url { get; set; } = string.Empty;
    public string Mime { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Folder { get; set; }
    public string? AltText { get; set; }
    public string? Tags { get; set; }
}
