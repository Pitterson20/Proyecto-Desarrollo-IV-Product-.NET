using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

[Table("products")]
public class Product : AuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    [Column("brand_id")]
    public int BrandId { get; set; }

    public Brand Brand { get; set; } = null!;

    [Column("unit_id")]
    public int UnitId { get; set; }

    public Unit Unit { get; set; } = null!;

    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("sku")]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Column("barcode")]
    [MaxLength(100)]
    public string? Barcode { get; set; }

    [Column("price", TypeName = "decimal(12,2)")]
    public decimal Price { get; set; }

    [Column("tax_rate", TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 0.13m;

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Column("minimum_stock")]
    public int MinimumStock { get; set; } = 1;

    [Column("maximum_stock")]
    public int? MaximumStock { get; set; }

    [Column("weight", TypeName = "decimal(8,2)")]
    public decimal? Weight { get; set; }

    [Column("image_url")]
    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
