using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Ferreteria.Auth;

/// <summary>
/// Conserva los mensajes en español para 401 y 403, igual que el manejador
/// global de Laravel y el filtro de NestJS.
/// </summary>
public sealed class ApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "No autenticado."
                });
            }

            return;
        }

        if (authorizeResult.Forbidden)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "No tiene permiso para realizar esta acción."
                });
            }

            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
