namespace Ferreteria.Common.Errors;

/// <summary>
/// Regla de negocio que se traduce a HTTP 409.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

/// <summary>
/// R1: un producto con ítems de compra no puede eliminarse.
/// </summary>
public sealed class ProductHasDependenciesException : BusinessRuleException
{
    public ProductHasDependenciesException()
        : base("No se puede eliminar el producto porque tiene dependencias activas.")
    {
    }
}

/// <summary>
/// R2: coherencia entre stock mínimo, stock actual y stock máximo.
/// </summary>
public sealed class InvalidProductStockException : BusinessRuleException
{
    public InvalidProductStockException(string message) : base(message)
    {
    }
}
