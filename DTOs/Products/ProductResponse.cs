using System.Globalization;
using Ferreteria.Models;

namespace Ferreteria.DTOs.Products;

/// <summary>
/// Equivalente a ProductResource.php: claves en español y peso decimal como
/// cadena para conservar el contrato de salida.
/// </summary>
public sealed class ProductResponse
{
    public int Id { get; init; }

    public int IdCategoria { get; init; }

    public int IdMarca { get; init; }

    public int IdUnidad { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string? Descripcion { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string? CodigoBarras { get; init; }

    public decimal Precio { get; init; }

    public decimal TasaImpuesto { get; init; }

    public int CantidadStock { get; init; }

    public int StockMinimo { get; init; }

    public int? StockMaximo { get; init; }

    public string? Peso { get; init; }

    public bool EstaActivo { get; init; }

    public static ProductResponse FromEntity(Product product) => new()
    {
        Id = product.Id,
        IdCategoria = product.CategoryId,
        IdMarca = product.BrandId,
        IdUnidad = product.UnitId,
        Nombre = product.Name,
        Descripcion = product.Description,
        Sku = product.Sku,
        CodigoBarras = product.Barcode,
        Precio = product.Price,
        TasaImpuesto = product.TaxRate,
        CantidadStock = product.StockQuantity,
        StockMinimo = product.MinimumStock,
        StockMaximo = product.MaximumStock,
        Peso = product.Weight?.ToString("0.00", CultureInfo.InvariantCulture),
        EstaActivo = product.IsActive
    };
}
