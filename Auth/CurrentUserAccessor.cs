using System.Security.Claims;
using Ferreteria.Models;

namespace Ferreteria.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser? User { get; }
}

public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser? User
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var idValue = principal.FindFirstValue("sub")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idValue, out var id))
            {
                return null;
            }

            var roleValue = principal.FindFirstValue("role") ?? "cajero";
            var abilities = principal.FindAll("abilities").Select(claim => claim.Value).ToArray();
            var expiresValue = principal.FindFirstValue("exp");

            DateTimeOffset? expiresAt = long.TryParse(expiresValue, out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds)
                : null;

            return new CurrentUser
            {
                Id = id,
                Email = principal.FindFirstValue("email") ?? string.Empty,
                Role = Enum.TryParse<UserRole>(roleValue, true, out var role)
                    ? role
                    : UserRole.Cajero,
                Abilities = abilities,
                Jti = principal.FindFirstValue("jti") ?? string.Empty,
                ExpiresAt = expiresAt
            };
        }
    }
}
