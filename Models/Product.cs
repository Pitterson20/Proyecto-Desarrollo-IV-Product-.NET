using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("id_category")]
    public int IdCategory { get; set; }

    [Column("id_brand")]
    public int IdBrand { get; set; }

    [Column("id_unit")]
    public int IdUnit { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Column("product_price", TypeName = "decimal(18,2)")]
    public decimal ProductPrice { get; set; }

    [Column("minium_stock")]
    public int MinimumStock { get; set; }

    [Column("maximum_stock")]
    public int MaximumStock { get; set; }

    [Column("tax_rate", TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; }

    [Column("bar_code")]
    public string? BarCode { get; set; }

    [Column("sku")]
    public string Sku { get; set; } = string.Empty;

    [Column("weight", TypeName = "decimal(8,2)")]
    public decimal Weight { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}