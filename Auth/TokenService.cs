using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ferreteria.DTOs.Auth;
using Ferreteria.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ferreteria.Auth;

public sealed class TokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public LoginResponse Create(User user)
    {
        var abilities = Permissions.ForRole(user.Role);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(_settings.ExpirationHours);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new("role", user.Role.ToString().ToLowerInvariant()),
            new("jti", jti)
        };

        claims.AddRange(abilities.Select(ability => new Claim("abilities", ability)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresAt = expiresAt.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            Abilities = abilities
        };
    }
}
