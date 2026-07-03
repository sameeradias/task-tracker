using backend.Models;

namespace backend.Services.Authorization;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleByIdAsync(int id);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<Role> CreateRoleAsync(string name, string? description);
    Task<Role?> UpdateRoleAsync(int id, string name, string? description);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<Role?> GetUserRoleAsync(int userId);
}