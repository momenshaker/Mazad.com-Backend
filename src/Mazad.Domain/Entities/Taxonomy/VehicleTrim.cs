using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Taxonomy;

public class VehicleTrim : AuditableEntity
{
    public Guid ModelId { get; set; }
    public string Name { get; set; } = string.Empty;

    public VehicleModel? Model { get; set; }
}
