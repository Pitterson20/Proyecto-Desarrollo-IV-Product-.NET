using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

[Table("units")]
public class Unit : AuditableEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("unit_name")]
    [MaxLength(150)]
    public string UnitName { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
