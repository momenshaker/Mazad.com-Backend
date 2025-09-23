using Mazad.Domain.Entities.Identity;
using Mazad.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Mazad.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.None;
    public bool IsDeleted { get; set; }
    public UserProfile? Profile { get; set; }
}
