using FluentAssertions;
using Xunit;
using backend.Models;
using backend.Services.UserService;
using backend.Tests.Helpers;

namespace backend.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_ShouldCreateWithHashedPassword()
    {
        var context = TestDbContextFactory.Create();
        var service = new UserService(context);

        var user = await service.CreateUserAsync("Jane", "Doe", "jane@test.com", "Password@123");

        user.Should().NotBeNull();
        user.PasswordHash.Should().NotBe("Password@123");
        BCrypt.Net.BCrypt.Verify("Password@123", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_ShouldThrow_WhenEmailExists()
    {
        var (context, _) = TestDbContextFactory.CreateWithTestUser();
        var service = new UserService(context);

        var act = async () => await service.CreateUserAsync("Another", "User", "test@example.com", "Pass@123");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenExists()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new UserService(context);

        var result = await service.GetUserByIdAsync(userId);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Test");
    }

    [Fact]
    public async Task GetAllUsers_ShouldExcludeSuperAdmin()
    {
        var context = TestDbContextFactory.Create();
        // Create Super Admin role and user
        var saRole = new Role { Name = "Super Admin", Description = "SA", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Roles.Add(saRole);
        await context.SaveChangesAsync();

        var saUser = new User { FirstName = "SA", Email = "sa@test.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Users.Add(saUser);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = saUser.Id, RoleId = saRole.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Create normal user
        var normalUser = new User { FirstName = "Normal", Email = "normal@test.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Users.Add(normalUser);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new UserService(context);
        var users = await service.GetAllUsersAsync();

        users.Should().HaveCount(1);
        users.Any(u => u.FirstName == "SA").Should().BeFalse();
        users.Any(u => u.FirstName == "Normal").Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateOnlyProvidedFields()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new UserService(context);

        var updated = await service.UpdateUserAsync(userId, "Updated", null, null);

        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("Updated");
        updated.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task DeleteUser_ShouldSoftDelete()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new UserService(context);

        var result = await service.DeleteUserAsync(userId);

        result.Should().BeTrue();
        var found = await service.GetUserByIdAsync(userId);
        found.Should().BeNull();
    }

    [Fact]
    public async Task AssignRole_ShouldAssignRoleToUser()
    {
        var (context, userId, roleId) = TestDbContextFactory.CreateWithTestUserAndRole();
        var service = new UserService(context);

        // Remove existing role first, then assign again
        await service.RemoveRoleFromUserAsync(userId, roleId);
        var result = await service.AssignRoleToUserAsync(userId, roleId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRole_ShouldRejectSuperAdminRole()
    {
        var context = TestDbContextFactory.Create();
        var saRole = new Role { Name = "Super Admin", Description = "SA", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Roles.Add(saRole);
        await context.SaveChangesAsync();

        var user = new User { FirstName = "Test", Email = "t@t.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new UserService(context);

        var act = async () => await service.AssignRoleToUserAsync(user.Id, saRole.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}