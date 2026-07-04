using Microsoft.EntityFrameworkCore;
using backend.Models;
using BCrypt.Net;

namespace backend.Data;

/// <summary>
/// Database initializer to seed default permissions, roles, and admin user.
/// All seeding operations are idempotent and safe to run multiple times.
/// </summary>
public class DbInitializer
{
    private readonly ApplicationDbContext _context;

    public DbInitializer(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds all permissions, roles, role-permission assignments, and default admin user.
    /// This method is idempotent and safe to run multiple times.
    /// </summary>
    public async Task SeedAsync()
    {
        Console.WriteLine("Starting database seeding...");

        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
        await SeedDefaultAdminUserAsync();

        Console.WriteLine("Database seeding completed successfully.");
    }

    private async Task SeedPermissionsAsync()
    {
        Console.WriteLine("Seeding permissions...");

        var permissions = new List<(string Name, string Description, string Category)>
        {
            // TASK module permissions
            ("READ:TASK", "Read tasks created by current user", "TASK"),
            ("READ_OTHERS:TASK", "Read tasks created by other users", "TASK"),
            ("CREATE:TASK", "Create new tasks", "TASK"),
            ("EDIT:TASK", "Edit tasks created by current user", "TASK"),
            ("EDIT_OTHERS:TASK", "Edit tasks created by other users", "TASK"),
            ("DELETE:TASK", "Delete tasks created by current user", "TASK"),
            ("DELETE_OTHERS:TASK", "Delete tasks created by other users", "TASK"),
            ("VIEW_MODULE:TASK", "Access task management module", "TASK"),

            // USER module permissions
            ("READ:USER", "Read own user profile", "USER"),
            ("READ_OTHERS:USER", "Read other users' profiles", "USER"),
            ("CREATE:USER", "Create new users", "USER"),
            ("EDIT:USER", "Edit own user profile", "USER"),
            ("EDIT_OTHERS:USER", "Edit other users' profiles", "USER"),
            ("DELETE:USER", "Delete own user account", "USER"),
            ("DELETE_OTHERS:USER", "Delete other users' accounts", "USER"),
            ("VIEW_MODULE:USER", "Access user management module", "USER"),

            // ROLE module permissions
            ("READ:ROLE", "Read role information", "ROLE"),
            ("CREATE:ROLE", "Create new roles", "ROLE"),
            ("EDIT:ROLE", "Edit existing roles", "ROLE"),
            ("DELETE:ROLE", "Delete roles", "ROLE"),
            ("ASSIGN:ROLE", "Assign roles to users", "ROLE"),
            ("VIEW_MODULE:ROLE", "Access role management module", "ROLE"),

            // PERMISSION module permissions
            ("READ:PERMISSION", "Read permission information", "PERMISSION"),
            ("VIEW_MODULE:PERMISSION", "Access permission management module", "PERMISSION")
        };

        var now = DateTime.UtcNow;
        var addedCount = 0;

        foreach (var (name, description, category) in permissions)
        {
            var existingPermission = await _context.Permissions
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Name == name);

            if (existingPermission == null)
            {
                var permission = new Permission
                {
                    Name = name,
                    Description = description,
                    Category = category,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Permissions.Add(permission);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($"Added {addedCount} new permissions.");
        }
        else
        {
            Console.WriteLine("All permissions already exist.");
        }
    }

    private async Task SeedRolesAsync()
    {
        Console.WriteLine("Seeding roles...");

        var roles = new List<(string Name, string Description)>
        {
            ("User", "Standard user who can manage their own tasks"),
            ("Admin", "Administrator who can manage all tasks and users"),
            ("Super Admin", "System super administrator - hidden from UI")
        };

        var now = DateTime.UtcNow;
        var addedCount = 0;

        foreach (var (name, description) in roles)
        {
            var existingRole = await _context.Roles
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Name == name);

            if (existingRole == null)
            {
                var role = new Role
                {
                    Name = name,
                    Description = description,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Roles.Add(role);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($"Added {addedCount} new roles.");
        }
        else
        {
            Console.WriteLine("All roles already exist.");
        }
    }

    private async Task SeedRolePermissionsAsync()
    {
        Console.WriteLine("Seeding role-permission assignments...");

        // Get all roles and permissions with tracking enabled
        var userRole = await _context.Roles
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Name == "User");
        var adminRole = await _context.Roles
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Name == "Admin");
        var superAdminRole = await _context.Roles
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Name == "Super Admin");

        var allPermissions = await _context.Permissions
            .AsTracking()
            .ToListAsync();

        if (userRole == null || adminRole == null || superAdminRole == null)
        {
            throw new InvalidOperationException("Required roles (User, Admin, Super Admin) not found. Ensure roles are seeded first.");
        }

        var now = DateTime.UtcNow;
        var addedCount = 0;

        // User role permissions
        var userPermissionNames = new[]
        {
            "READ:TASK", "CREATE:TASK", "EDIT:TASK", "DELETE:TASK", "VIEW_MODULE:TASK",
            "READ:USER", "EDIT:USER"
        };

        foreach (var permissionName in userPermissionNames)
        {
            var permission = allPermissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission != null)
            {
                var existingAssignment = await _context.RolePermissions
                    .AsTracking()
                    .FirstOrDefaultAsync(rp => rp.RoleId == userRole.Id && rp.PermissionId == permission.Id);

                if (existingAssignment == null)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = userRole.Id,
                        PermissionId = permission.Id,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    _context.RolePermissions.Add(rolePermission);
                    addedCount++;
                }
            }
        }

        // Admin role permissions (all permissions)
        foreach (var permission in allPermissions)
        {
            var existingAssignment = await _context.RolePermissions
                .AsTracking()
                .FirstOrDefaultAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);

            if (existingAssignment == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.RolePermissions.Add(rolePermission);
                addedCount++;
            }
        }

        // Super Admin role permissions (all permissions)
        foreach (var permission in allPermissions)
        {
            var existingAssignment = await _context.RolePermissions
                .AsTracking()
                .FirstOrDefaultAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);

            if (existingAssignment == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.RolePermissions.Add(rolePermission);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync();
            Console.WriteLine($"Added {addedCount} new role-permission assignments.");
        }
        else
        {
            Console.WriteLine("All role-permission assignments already exist.");
        }
    }

    private async Task SeedDefaultAdminUserAsync()
    {
        Console.WriteLine("Seeding default admin user...");

        var adminEmail = "admin@tasktracker.com";
        var existingAdmin = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin != null)
        {
            Console.WriteLine("Default admin user already exists.");
            return;
        }

        var superAdminRole = await _context.Roles
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Name == "Super Admin");

        if (superAdminRole == null)
        {
            throw new InvalidOperationException("Super Admin role not found. Ensure roles are seeded first.");
        }

        var now = DateTime.UtcNow;

        // Create admin user with properly hashed password
        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        // Assign Super Admin role to the user
        var userRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdminRole.Id,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        Console.WriteLine("Created default admin user and assigned Super Admin role.");
    }
}