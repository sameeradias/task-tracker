using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

/// <summary>
/// Permission entity representing specific actions that can be performed in the system.
/// Format: ACTION:MODULE (e.g., "READ:TASK", "CREATE:USER")
/// </summary>
public class Permission : BaseEntity
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public required string Category { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}