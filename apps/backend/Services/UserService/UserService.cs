using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Services.UserService;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUserAsync(string firstName, string lastName, string email, string password)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with email '{email}' already exists.");
        }

        // Hash the password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<User?> UpdateUserAsync(int id, string? firstName, string? lastName, string? email)
    {
        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        // Check if email is being changed and if it already exists
        if (email != null && email != user.Email)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with email '{email}' already exists.");
            }
            user.Email = email;
        }

        if (firstName != null)
            user.FirstName = firstName;

        if (lastName != null)
            user.LastName = lastName;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return false;

        // Soft delete
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        // Check if user exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        // Check if role exists and prevent assignment of Super Admin role
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null)
            return false;

        if (string.Equals(role.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot assign 'Super Admin' role - it is a system role.");
        }

        // Remove existing role first (one role per user)
        var existingUserRoles = await _context.UserRoles
            .AsTracking()
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        if (existingUserRoles.Any())
        {
            // Soft delete existing roles
            foreach (var existingRole in existingUserRoles)
            {
                existingRole.IsDeleted = true;
                existingRole.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Add new role
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