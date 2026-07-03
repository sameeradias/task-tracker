using backend.Models;

namespace backend.Services.UserService;

public interface IUserService
{
    Task<User> CreateUserAsync(string firstName, string lastName, string email, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> UpdateUserAsync(int id, string? firstName, string? lastName, string? email);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<Role?> GetUserRoleAsync(int userId);
}