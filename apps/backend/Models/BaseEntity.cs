using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

/// <summary>
/// Base entity class providing common audit fields for all entities.
/// </summary>
public abstract class BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CreatedByUser))]
    public int? CreatedBy { get; set; }

    [ForeignKey(nameof(LastUpdatedByUser))]
    public int? LastUpdatedBy { get; set; }

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties for audit fields
    // These will be configured in the DbContext to avoid circular references
    [ForeignKey(nameof(CreatedBy))]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey(nameof(LastUpdatedBy))]
    public virtual User? LastUpdatedByUser { get; set; }
}