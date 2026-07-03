using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

/// <summary>
/// User entity representing application users with authentication and profile information.
/// </summary>
public class User : BaseEntity
{
    [Required]
    [StringLength(100)]
    public required string FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(512)]
    public required string PasswordHash { get; set; }

    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    // Reverse navigation for audit fields
    public virtual ICollection<BaseEntity> CreatedEntities { get; set; } = new List<BaseEntity>();
    public virtual ICollection<BaseEntity> LastUpdatedEntities { get; set; } = new List<BaseEntity>();
}