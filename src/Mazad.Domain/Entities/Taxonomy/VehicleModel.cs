using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Taxonomy;

public class VehicleModel : AuditableEntity
{
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public VehicleBrand? Brand { get; set; }
    public ICollection<VehicleTrim> Trims { get; set; } = new List<VehicleTrim>();
}
