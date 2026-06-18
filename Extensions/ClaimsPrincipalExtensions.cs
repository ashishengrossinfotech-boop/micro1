using System.Security.Claims;

namespace LoginDemo.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetDisplayName(this ClaimsPrincipal user)
    {
        var fullName = user.FindFirst("FullName")?.Value;
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        var identityName = user.Identity?.Name;
        return string.IsNullOrWhiteSpace(identityName) ? "there" : identityName;
    }
}
