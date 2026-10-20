using System.Collections.Concurrent;

namespace Ferreteria.Auth;

/// <summary>
/// Equivalente en memoria a la revocación de tokens de Sanctum: el logout
/// invalida el jti del token hasta su expiración.
/// </summary>
public sealed class TokenRevocationStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revoked = new();

    public void Revoke(string jti, DateTimeOffset expiresAt)
    {
        if (!string.IsNullOrWhiteSpace(jti))
        {
            _revoked[jti] = expiresAt;
        }
    }

    public bool IsRevoked(string jti)
    {
        if (!_revoked.TryGetValue(jti, out var expiresAt))
        {
            return false;
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            _revoked.TryRemove(jti, out _);
            return false;
        }

        return true;
    }
}
