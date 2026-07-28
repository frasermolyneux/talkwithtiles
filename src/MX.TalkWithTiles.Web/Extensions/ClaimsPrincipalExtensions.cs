using System.Security.Claims;

namespace MX.TalkWithTiles.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.IsInRole("Admin");
    }

    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static Guid GetUserGuid(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        var identifier = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("No NameIdentifier claim found.");

        // Entra ID work accounts return a GUID directly; personal Microsoft accounts
        // may return a non-GUID string. Produce a deterministic GUID in all cases.
        if (Guid.TryParse(identifier, out var guid))
        {
            return guid;
        }

        // Generate a deterministic GUID (v5-style) from the identifier string
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(identifier));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }

    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("name")?.Value
            ?? principal.Identity?.Name;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("preferred_username")?.Value;
    }
}
