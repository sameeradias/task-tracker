using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Services.PermissionService;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category)
    {
        return await _context.Permissions
            .Where(p => EF.Functions.ILike(p.Category, category))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Permission?> GetPermissionByNameAsync(string name)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        var permissionNames = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions, 
                ur => ur.RoleId, 
                rp => rp.RoleId, 
                (ur, rp) => rp)
            .Join(_context.Permissions, 
                rp => rp.PermissionId, 
                p => p.Id, 
                (rp, p) => p.Name)
            .Distinct()
            .ToListAsync();

        return permissionNames;
    }
}