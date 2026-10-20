namespace Ferreteria.Common.Errors;

/// <summary>
/// Límite de intentos de inicio de sesión. Se traduce a HTTP 429 y agrega
/// la cabecera Retry-After.
/// </summary>
public sealed class TooManyRequestsException : Exception
{
    public TooManyRequestsException(string message, int retryAfterSeconds) : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; }
}
