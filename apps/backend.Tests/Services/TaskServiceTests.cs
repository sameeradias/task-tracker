using FluentAssertions;
using Xunit;
using backend.Models;
using backend.Models.Enums;
using backend.Services.TaskService;
using backend.Tests.Helpers;

namespace backend.Tests.Services;

public class TaskServiceTests
{
    [Fact]
    public async Task CreateTask_ShouldCreateAndReturnTask()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);

        var result = await service.CreateTaskAsync(userId, "Test Task", "Description", TaskItemStatus.Todo, null);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Description");
        result.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task CreateTask_ShouldSetOwnerIdCorrectly()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);

        var result = await service.CreateTaskAsync(userId, "My Task", null, TaskItemStatus.Todo, null);

        result.OwnerId.Should().Be(userId);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnTask_WhenExists()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);
        var created = await service.CreateTaskAsync(userId, "Find Me", null, TaskItemStatus.Todo, null);

        var result = await service.GetTaskByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnNull_WhenNotExists()
    {
        var context = TestDbContextFactory.Create();
        var service = new TaskService(context);

        var result = await service.GetTaskByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTasks_ShouldReturnPaginatedResults()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);

        for (int i = 0; i < 15; i++)
            await service.CreateTaskAsync(userId, $"Task {i}", null, TaskItemStatus.Todo, null);

        var (items, totalCount) = await service.GetTasksAsync(null, null, 1, 10);

        totalCount.Should().Be(15);
        items.Count().Should().Be(10);
    }

    [Fact]
    public async Task GetTasks_ShouldFilterByStatus()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);

        await service.CreateTaskAsync(userId, "Todo Task", null, TaskItemStatus.Todo, null);
        await service.CreateTaskAsync(userId, "Done Task", null, TaskItemStatus.Done, null);
        await service.CreateTaskAsync(userId, "InProgress Task", null, TaskItemStatus.InProgress, null);

        var (items, totalCount) = await service.GetTasksAsync(null, TaskItemStatus.Done, 1, 10);

        totalCount.Should().Be(1);
        items.First().Title.Should().Be("Done Task");
    }

    [Fact]
    public async Task GetTasks_ShouldFilterByOwnerId()
    {
        var context = TestDbContextFactory.Create();
        var user1 = new User { FirstName = "User1", Email = "u1@test.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var user2 = new User { FirstName = "User2", Email = "u2@test.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new TaskService(context);
        await service.CreateTaskAsync(user1.Id, "User1 Task", null, TaskItemStatus.Todo, null);
        await service.CreateTaskAsync(user2.Id, "User2 Task", null, TaskItemStatus.Todo, null);

        var (items, totalCount) = await service.GetTasksAsync(user1.Id, null, 1, 10);

        totalCount.Should().Be(1);
        items.First().Title.Should().Be("User1 Task");
    }

    [Fact]
    public async Task GetTasks_ShouldOrderByCreatedAtDescending()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);

        await service.CreateTaskAsync(userId, "First", null, TaskItemStatus.Todo, null);
        await Task.Delay(10);
        await service.CreateTaskAsync(userId, "Second", null, TaskItemStatus.Todo, null);

        var (items, _) = await service.GetTasksAsync(null, null, 1, 10);
        var list = items.ToList();

        list[0].Title.Should().Be("Second");
        list[1].Title.Should().Be("First");
    }

    [Fact]
    public async Task UpdateTask_ShouldUpdateOnlyProvidedFields()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);
        var task = await service.CreateTaskAsync(userId, "Original", "Original Desc", TaskItemStatus.Todo, null);

        var updated = await service.UpdateTaskAsync(task.Id, "Updated Title", null, null, null);

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated Title");
        updated.Description.Should().Be("Original Desc");
        updated.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnNull_WhenNotExists()
    {
        var context = TestDbContextFactory.Create();
        var service = new TaskService(context);

        var result = await service.UpdateTaskAsync(999, "Title", null, null, null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_ShouldSoftDelete()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);
        var task = await service.CreateTaskAsync(userId, "Delete Me", null, TaskItemStatus.Todo, null);

        var result = await service.DeleteTaskAsync(task.Id);

        result.Should().BeTrue();
        // Task should not be findable via normal query (soft delete filter)
        var found = await service.GetTaskByIdAsync(task.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnFalse_WhenNotExists()
    {
        var context = TestDbContextFactory.Create();
        var service = new TaskService(context);

        var result = await service.DeleteTaskAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTaskOwner_ShouldReturnTrue_WhenOwner()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);
        var task = await service.CreateTaskAsync(userId, "My Task", null, TaskItemStatus.Todo, null);

        var result = await service.IsTaskOwnerAsync(task.Id, userId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTaskOwner_ShouldReturnFalse_WhenNotOwner()
    {
        var (context, userId) = TestDbContextFactory.CreateWithTestUser();
        var service = new TaskService(context);
        var task = await service.CreateTaskAsync(userId, "My Task", null, TaskItemStatus.Todo, null);

        var result = await service.IsTaskOwnerAsync(task.Id, userId + 100);

        result.Should().BeFalse();
    }
}