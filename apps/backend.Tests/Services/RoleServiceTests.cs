using FluentAssertions;
using Xunit;
using backend.Models;
using backend.Services.RoleService;
using backend.Tests.Helpers;

namespace backend.Tests.Services;

public class RoleServiceTests
{
    [Fact]
    public async Task GetAllRoles_ShouldExcludeSuperAdmin()
    {
        var context = TestDbContextFactory.Create();
        context.Roles.Add(new Role { Name = "Super Admin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.Roles.Add(new Role { Name = "User", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new RoleService(context);
        var roles = await service.GetAllRolesAsync();

        roles.Should().HaveCount(1);
        roles.First().Name.Should().Be("User");
    }

    [Fact]
    public async Task CreateRole_ShouldCreateRole()
    {
        var context = TestDbContextFactory.Create();
        var service = new RoleService(context);

        var role = await service.CreateRoleAsync("Manager", "A manager role");

        role.Should().NotBeNull();
        role.Name.Should().Be("Manager");
        role.Description.Should().Be("A manager role");
    }

    [Fact]
    public async Task CreateRole_ShouldRejectSuperAdminName()
    {
        var context = TestDbContextFactory.Create();
        var service = new RoleService(context);

        var act = async () => await service.CreateRoleAsync("Super Admin", "Trying to create SA");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateRole_ShouldUpdateFields()
    {
        var context = TestDbContextFactory.Create();
        var service = new RoleService(context);
        var role = await service.CreateRoleAsync("OldName", "OldDesc");

        var updated = await service.UpdateRoleAsync(role.Id, "NewName", "NewDesc");

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("NewName");
        updated.Description.Should().Be("NewDesc");
    }

    [Fact]
    public async Task DeleteRole_ShouldSoftDelete()
    {
        var context = TestDbContextFactory.Create();
        var service = new RoleService(context);
        var role = await service.CreateRoleAsync("ToDelete", null);

        var result = await service.DeleteRoleAsync(role.Id);

        result.Should().BeTrue();
        var found = await service.GetRoleByIdAsync(role.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRole_ShouldRejectSuperAdminDeletion()
    {
        var context = TestDbContextFactory.Create();
        context.Roles.Add(new Role { Name = "Super Admin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var saRole = context.Roles.First(r => r.Name == "Super Admin");
        context.ChangeTracker.Clear();

        var service = new RoleService(context);
        var act = async () => await service.DeleteRoleAsync(saRole.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AssignPermissions_ShouldReplaceExistingPermissions()
    {
        var context = TestDbContextFactory.Create();
        var perm1 = new Permission { Name = "READ:TASK", Category = "TASK", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var perm2 = new Permission { Name = "CREATE:TASK", Category = "TASK", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Permissions.AddRange(perm1, perm2);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new RoleService(context);
        var role = await service.CreateRoleAsync("TestRole", null);

        // Assign first permission
        await service.AssignPermissionsToRoleAsync(role.Id, new[] { perm1.Id });
        var perms1 = await service.GetRolePermissionsAsync(role.Id);
        perms1.Should().HaveCount(1);

        // Replace with second permission
        await service.AssignPermissionsToRoleAsync(role.Id, new[] { perm2.Id });
        var perms2 = await service.GetRolePermissionsAsync(role.Id);
        perms2.Should().HaveCount(1);
        perms2.First().Name.Should().Be("CREATE:TASK");
    }
}