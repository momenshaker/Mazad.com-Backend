using System.Security.Claims;
using System;
using System.Linq;

namespace Mazad.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
    }

    public static bool HasScope(this ClaimsPrincipal principal, string scope)
    {
        if (principal.Identity is null || !principal.Identity.IsAuthenticated)
        {
            return false;
        }

        var scopeClaims = principal.FindAll("scope")
            .Concat(principal.FindAll("http://schemas.microsoft.com/identity/claims/scope"));

        foreach (var claim in scopeClaims)
        {
            if (string.IsNullOrWhiteSpace(claim.Value))
            {
                continue;
            }

            var values = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (values.Any(v => string.Equals(v, scope, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
