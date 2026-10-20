using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.IdentityModel.Tokens;

namespace Ferreteria.Auth;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("Falta la configuración JWT.");

        if (string.IsNullOrWhiteSpace(settings.Secret)
            || Encoding.UTF8.GetByteCount(settings.Secret) < 32)
        {
            throw new InvalidOperationException(
                "La clave JWT debe tener al menos 32 caracteres.");
        }

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(settings.Secret));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "No autenticado."
                        });
                    },
                    OnTokenValidated = context =>
                    {
                        var jti = context.Principal?.FindFirst("jti")?.Value;
                        var revocation = context.HttpContext.RequestServices
                            .GetRequiredService<TokenRevocationStore>();

                        if (!string.IsNullOrWhiteSpace(jti) && revocation.IsRevoked(jti))
                        {
                            context.Fail("El token fue revocado.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var ability in Permissions.ProductAbilities)
            {
                options.AddPolicy(ability, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim("abilities", ability));
            }
        });

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();
        services.AddSingleton<LoginAttemptLimiter>();
        services.AddSingleton<TokenRevocationStore>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();

        return services;
    }
}
