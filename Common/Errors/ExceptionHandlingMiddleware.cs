using System.Text.Json;
using Ferreteria.Common.Errors;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Ferreteria.Common.Errors;

/// <summary>
/// Mapeo central de excepciones equivalente al manejador de Laravel y al
/// filtro global de NestJS. Nunca expone la traza al cliente.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions FallbackJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IOptions<JsonOptions> jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions.Value.SerializerOptions ?? FallbackJsonOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiValidationException exception)
        {
            await WriteAsync(context, StatusCodes.Status422UnprocessableEntity, new
            {
                message = exception.Message,
                errors = exception.Errors
            });
        }
        catch (BusinessRuleException exception)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, new
            {
                message = exception.Message
            });
        }
        catch (ResourceNotFoundException exception)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, new
            {
                message = exception.Message
            });
        }
        catch (ForbiddenAccessException exception)
        {
            await WriteAsync(context, StatusCodes.Status403Forbidden, new
            {
                message = exception.Message
            });
        }
        catch (UnauthorizedException exception)
        {
            await WriteAsync(context, StatusCodes.Status401Unauthorized, new
            {
                message = exception.Message
            });
        }
        catch (TooManyRequestsException exception)
        {
            await WriteAsync(
                context,
                StatusCodes.Status429TooManyRequests,
                new { message = exception.Message },
                exception.RetryAfterSeconds);
        }
        catch (UnauthorizedAccessException)
        {
            await WriteAsync(context, StatusCodes.Status401Unauthorized, new
            {
                message = "No autenticado."
            });
        }
        catch (JsonException)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, new
            {
                message = "El cuerpo JSON enviado no es válido."
            });
        }
        catch (BadHttpRequestException)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, new
            {
                message = "El cuerpo JSON enviado no es válido."
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error no controlado al procesar la solicitud.");

            await WriteAsync(context, StatusCodes.Status500InternalServerError, new
            {
                message = "Ocurrió un error interno. Intente de nuevo más tarde."
            });
        }
    }

    private async Task WriteAsync(
        HttpContext context,
        int statusCode,
        object body,
        int? retryAfterSeconds = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        if (retryAfterSeconds is not null)
        {
            context.Response.Headers.RetryAfter = retryAfterSeconds.Value.ToString();
        }

        await JsonSerializer.SerializeAsync(context.Response.Body, body, _jsonOptions);
    }
}
