using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Taxonomy;

public class YearRange : AuditableEntity
{
    public int StartYear { get; set; }
    public int EndYear { get; set; }
}
