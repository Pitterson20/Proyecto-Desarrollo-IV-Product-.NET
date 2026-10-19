namespace Ferreteria.Common.Errors;

/// <summary>
/// Errores de validación con la misma forma que Laravel/NestJS:
/// una lista de mensajes por cada campo.
/// </summary>
public sealed class ApiValidationException : Exception
{
    public ApiValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Los datos enviados no son válidos.")
    {
        Errors = errors;
    }

    public ApiValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
