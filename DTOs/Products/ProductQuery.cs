using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.DTOs.Products;

public sealed class ProductQuery
{
    [FromQuery(Name = "page")]
    public string? Page { get; set; }

    [FromQuery(Name = "per_page")]
    public string? PerPage { get; set; }

    [FromQuery(Name = "q")]
    public string? Q { get; set; }

    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    [FromQuery(Name = "direction")]
    public string? Direction { get; set; }

    [FromQuery(Name = "category_id")]
    public string? CategoryId { get; set; }

    [FromQuery(Name = "brand_id")]
    public string? BrandId { get; set; }

    [FromQuery(Name = "is_active")]
    public string? IsActive { get; set; }

    [FromQuery(Name = "low_stock")]
    public string? LowStock { get; set; }

    [FromQuery(Name = "min_price")]
    public string? MinPrice { get; set; }

    [FromQuery(Name = "max_price")]
    public string? MaxPrice { get; set; }
}
