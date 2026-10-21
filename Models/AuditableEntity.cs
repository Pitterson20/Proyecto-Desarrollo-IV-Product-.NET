using System.ComponentModel.DataAnnotations.Schema;

namespace Ferreteria.Models;

/// <summary>
/// Campos de auditoría compartidos por las entidades que usan marcas de tiempo.
/// </summary>
public abstract class AuditableEntity
{
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
