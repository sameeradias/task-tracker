using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Enums;

namespace backend.Services.TaskService;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem> CreateTaskAsync(int ownerId, string title, string? description, TaskItemStatus status, DateTime? dueDate)
    {
        var taskItem = new TaskItem
        {
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate.HasValue ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc) : null,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = ownerId
        };

        _context.TaskItems.Add(taskItem);
        await _context.SaveChangesAsync();

        // Reload with navigation property
        return (await GetTaskByIdAsync(taskItem.Id))!;
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        return await _context.TaskItems
            .Include(t => t.Owner)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetTasksAsync(int? ownerId, TaskItemStatus? status, int page, int pageSize)
    {
        var query = _context.TaskItems
            .Include(t => t.Owner)
            .AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(t => t.OwnerId == ownerId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<TaskItem?> UpdateTaskAsync(int id, string? title, string? description, TaskItemStatus? status, DateTime? dueDate)
    {
        var taskItem = await _context.TaskItems
            .AsTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (taskItem == null)
            return null;

        if (title != null)
            taskItem.Title = title;
        if (description != null)
            taskItem.Description = description;
        if (status.HasValue)
            taskItem.Status = status.Value;
        if (dueDate.HasValue)
            taskItem.DueDate = DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc);

        taskItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with navigation
        return await GetTaskByIdAsync(taskItem.Id);
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var taskItem = await _context.TaskItems
            .AsTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (taskItem == null)
            return false;

        taskItem.IsDeleted = true;
        taskItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsTaskOwnerAsync(int taskId, int userId)
    {
        return await _context.TaskItems
            .AnyAsync(t => t.Id == taskId && t.OwnerId == userId);
    }
}