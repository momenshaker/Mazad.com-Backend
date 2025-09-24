using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Cms;

public class Menu : AuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Items { get; set; } = string.Empty;
}
