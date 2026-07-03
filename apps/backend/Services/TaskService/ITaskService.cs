using backend.Models;
using backend.Models.Enums;

namespace backend.Services.TaskService;

public interface ITaskService
{
    Task<TaskItem> CreateTaskAsync(int ownerId, string title, string? description, TaskItemStatus status, DateTime? dueDate);
    Task<TaskItem?> GetTaskByIdAsync(int id);
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetTasksAsync(int? ownerId, TaskItemStatus? status, int page, int pageSize);
    Task<TaskItem?> UpdateTaskAsync(int id, string? title, string? description, TaskItemStatus? status, DateTime? dueDate);
    Task<bool> DeleteTaskAsync(int id);
    Task<bool> IsTaskOwnerAsync(int taskId, int userId);
}