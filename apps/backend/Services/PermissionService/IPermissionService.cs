using backend.Models;

namespace backend.Services.PermissionService;

public interface IPermissionService
{
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category);
    Task<Permission?> GetPermissionByNameAsync(string name);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
}