using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ferreteria.DTOs.Products;

public sealed class CreateProductRequest
{
    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? UnitId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public int? StockQuantity { get; set; }

    public decimal? TaxRate { get; set; }

    public int? MinimumStock { get; set; }

    public int? MaximumStock { get; set; }

    public decimal? Weight { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}
