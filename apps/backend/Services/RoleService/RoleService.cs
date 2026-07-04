using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Services.RoleService;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles
            .Where(r => r.Name != "Super Admin")
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role> CreateRoleAsync(string name, string? description)
    {
        if (string.Equals(name, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot create 'Super Admin' role - it is a system role.");
        }

        var role = new Role 
        { 
            Name = name, 
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        
        return role;
    }

    public async Task<Role?> UpdateRoleAsync(int id, string name, string? description)
    {
        var role = await _context.Roles
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return null;

        if (string.Equals(role.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot modify 'Super Admin' role - it is a system role.");
        }

        if (string.Equals(name, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot rename role to 'Super Admin' - it is a system role.");
        }

        role.Name = name;
        role.Description = description;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return false;

        if (string.Equals(role.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot delete 'Super Admin' role - it is a system role.");
        }

        // Soft delete
        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds)
    {
        var role = await _context.Roles
            .AsTracking()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return false;

        // Remove existing permissions for this role
        var existingRolePermissions = await _context.RolePermissions
            .AsTracking()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingRolePermissions);

        // Add new permissions
        var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _context.RolePermissions.AddRangeAsync(newRolePermissions);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        // Check if the role being assigned is Super Admin
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role != null && string.Equals(role.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot assign 'Super Admin' role - it is a system role.");
        }

        // Check if the assignment already exists
        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingUserRole != null)
            return true; // Already assigned

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
            .AsTracking()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null)
            return false;

        // Soft delete
        userRole.IsDeleted = true;
        userRole.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Role?> GetUserRoleAsync(int userId)
    {
        var userRole = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync();

        return userRole?.Role;
    }
}