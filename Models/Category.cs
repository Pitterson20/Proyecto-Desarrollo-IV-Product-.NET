using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

[Table("categories")]
public class Category : AuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("category_name")]
    [MaxLength(150)]
    public string CategoryName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
