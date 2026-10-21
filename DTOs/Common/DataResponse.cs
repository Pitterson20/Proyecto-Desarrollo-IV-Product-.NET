namespace Ferreteria.DTOs.Common;

/// <summary>
/// Envoltura equivalente a la respuesta de NestJS para un recurso individual.
/// </summary>
public sealed class DataResponse<T>
{
    public T Data { get; init; } = default!;
}
