using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Admin;

public class AuditLogEntry : BaseEntity
{
    public string Actor { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
}
