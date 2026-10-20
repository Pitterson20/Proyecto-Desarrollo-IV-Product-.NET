using Ferreteria.Models;

namespace Ferreteria.Auth;

public sealed class CurrentUser
{
    public int Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public UserRole Role { get; init; }

    public IReadOnlyList<string> Abilities { get; init; } = [];

    public string Jti { get; init; } = string.Empty;

    public DateTimeOffset? ExpiresAt { get; init; }

    public bool HasAbility(string ability) => Abilities.Contains(ability);
}
