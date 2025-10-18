using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Taxonomy;

public class VehicleBrand : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<VehicleModel> Models { get; set; } = [];
}
