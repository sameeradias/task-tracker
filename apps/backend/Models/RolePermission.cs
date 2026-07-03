using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

/// <summary>
/// Junction entity representing the many-to-many relationship between Roles and Permissions.
/// </summary>
public class RolePermission : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Role))]
    public required int RoleId { get; set; }

    [Required]
    [ForeignKey(nameof(Permission))]
    public required int PermissionId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}