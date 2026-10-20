namespace Ferreteria.Common.Errors;

/// <summary>
/// Credenciales inválidas o token ausente. Se traduce a HTTP 401 con el
/// mensaje indicado.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
