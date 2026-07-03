using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

/// <summary>
/// Junction entity representing the many-to-many relationship between Users and Roles.
/// </summary>
public class UserRole : BaseEntity
{
    [Required]
    [ForeignKey(nameof(User))]
    public required int UserId { get; set; }

    [Required]
    [ForeignKey(nameof(Role))]
    public required int RoleId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}