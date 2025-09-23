using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Catalog;

public class AttributeDefinition : AuditableEntity
{
    public Guid CategoryId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? OptionsJson { get; set; }

    public Category? Category { get; set; }
}
