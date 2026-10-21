using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

[Table("brands")]
public class Brand : AuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("brand_name")]
    [MaxLength(150)]
    public string BrandName { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
