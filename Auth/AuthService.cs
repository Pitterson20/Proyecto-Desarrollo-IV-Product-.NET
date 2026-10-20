using Ferreteria.Common.Errors;
using Ferreteria.Data;
using Ferreteria.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Auth;

public sealed class AuthService
{
    public const string LogoutMessage = "Sesión cerrada correctamente.";

    private readonly AppDBContext _db;
    private readonly TokenService _tokenService;
    private readonly LoginAttemptLimiter _limiter;
    private readonly TokenRevocationStore _revocation;

    public AuthService(
        AppDBContext db,
        TokenService tokenService,
        LoginAttemptLimiter limiter,
        TokenRevocationStore revocation)
    {
        _db = db;
        _tokenService = tokenService;
        _limiter = limiter;
        _revocation = revocation;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var email = (request.Email ?? string.Empty).Trim();
        var key = $"{email.ToLowerInvariant()}|{ipAddress}";

        if (_limiter.IsLimited(key, out var retryAfterSeconds))
        {
            throw new TooManyRequestsException(
                "Demasiados intentos de inicio de sesión. Intente nuevamente más tarde.",
                retryAfterSeconds);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(
                candidate => candidate.Email.ToLower() == email.ToLower(),
                cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            _limiter.RegisterFailure(key);

            throw new UnauthorizedException(
                "Las credenciales proporcionadas son incorrectas.");
        }

        _limiter.Clear(key);

        return _tokenService.Create(user);
    }

    public void Logout(CurrentUser? user)
    {
        if (user is null || string.IsNullOrWhiteSpace(user.Jti))
        {
            return;
        }

        _revocation.Revoke(
            user.Jti,
            user.ExpiresAt ?? DateTimeOffset.UtcNow);
    }
}
