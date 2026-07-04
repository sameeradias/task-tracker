using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a context with a seeded test user for convenience.
    /// Returns (context, userId)
    /// </summary>
    public static (ApplicationDbContext Context, int UserId) CreateWithTestUser(string? dbName = null)
    {
        var context = Create(dbName);

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();

        // Detach so NoTracking doesn't interfere
        context.ChangeTracker.Clear();

        return (context, user.Id);
    }

    /// <summary>
    /// Creates a context with a test user and a role with permissions.
    /// </summary>
    public static (ApplicationDbContext Context, int UserId, int RoleId) CreateWithTestUserAndRole(string? dbName = null)
    {
        var context = Create(dbName);

        var role = new Role
        {
            Name = "TestRole",
            Description = "Test role",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Roles.Add(role);
        context.SaveChanges();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.UserRoles.Add(userRole);
        context.SaveChanges();

        context.ChangeTracker.Clear();

        return (context, user.Id, role.Id);
    }
}