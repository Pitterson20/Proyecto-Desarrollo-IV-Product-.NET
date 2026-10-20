namespace Ferreteria.DTOs.Auth;

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;

    public string TokenType { get; init; } = "Bearer";

    public string ExpiresAt { get; init; } = string.Empty;

    public IReadOnlyList<string> Abilities { get; init; } = [];
}
