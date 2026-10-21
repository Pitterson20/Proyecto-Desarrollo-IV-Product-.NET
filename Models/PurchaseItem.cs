using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

[Table("purchase_items")]
public class PurchaseItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Column("quantity")]
    public int Quantity { get; set; } = 1;
}
