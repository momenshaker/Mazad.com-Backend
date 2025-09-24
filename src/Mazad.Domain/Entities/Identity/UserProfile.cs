using Mazad.Domain.Common;

namespace Mazad.Domain.Entities.Identity;

public class UserProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
}
