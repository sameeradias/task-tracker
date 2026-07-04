using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using backend.Attributes;
using backend.DTOs.Task;
using backend.DTOs.Common;
using backend.Hubs;
using backend.Models.Enums;
using backend.Services.TaskService;

namespace backend.Controllers;

[Route("api/tasks")]
[ApiController]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IHubContext<TaskHub> _hubContext;

    public TaskController(ITaskService taskService, IHubContext<TaskHub> hubContext)
    {
        _taskService = taskService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get tasks with pagination and filtering.
    /// Regular users see only their own tasks. Admins can see all.
    /// </summary>
    [HttpGet]
    [PermissionGuard("READ:TASK", "READ_OTHERS:TASK")]
    public async Task<ActionResult<PaginatedResponseDTO<TaskResponseDTO>>> GetTasks([FromQuery] TaskFilterRequestDTO filter)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        // Regular users can only see their own tasks
        int? ownerFilter = isAdmin ? filter.OwnerId : userId;

        TaskItemStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<TaskItemStatus>(filter.Status, true, out var parsedStatus))
            statusFilter = parsedStatus;

        var (items, totalCount) = await _taskService.GetTasksAsync(ownerFilter, statusFilter, filter.Page, filter.PageSize);

        var taskDtos = items.Select(MapToDto);
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var response = new PaginatedResponseDTO<TaskResponseDTO>(
            taskDtos,
            totalCount,
            filter.Page,
            filter.PageSize,
            totalPages
        );

        return Ok(response);
    }

    /// <summary>
    /// Get a task by ID.
    /// Regular users can only view their own tasks.
    /// </summary>
    [HttpGet("{id}")]
    [PermissionGuard("READ:TASK", "READ_OTHERS:TASK")]
    public async Task<ActionResult<TaskResponseDTO>> GetTaskById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound(new { message = "Task not found." });

        if (!IsCurrentUserAdmin() && task.OwnerId != GetCurrentUserId())
            return Forbid();

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Create a new task. Owner is set to the current user.
    /// </summary>
    [HttpPost]
    [PermissionGuard("CREATE:TASK")]
    public async Task<ActionResult<TaskResponseDTO>> CreateTask(CreateTaskRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        var status = TaskItemStatus.Todo;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TaskItemStatus>(request.Status, true, out var parsedStatus))
            status = parsedStatus;

        var task = await _taskService.CreateTaskAsync(userId, request.Title, request.Description, status, request.DueDate);

        var responseDto = MapToDto(task);
        await _hubContext.Clients.All.SendAsync("TaskCreated", responseDto);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, responseDto);
    }

    /// <summary>
    /// Update a task. Regular users can only update their own tasks.
    /// </summary>
    [HttpPut("{id}")]
    [PermissionGuard("EDIT:TASK", "EDIT_OTHERS:TASK")]
    public async Task<ActionResult<TaskResponseDTO>> UpdateTask(int id, UpdateTaskRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check ownership for regular users
        if (!IsCurrentUserAdmin())
        {
            var isOwner = await _taskService.IsTaskOwnerAsync(id, GetCurrentUserId());
            if (!isOwner)
                return Forbid();
        }

        TaskItemStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TaskItemStatus>(request.Status, true, out var parsedStatus))
            status = parsedStatus;

        var task = await _taskService.UpdateTaskAsync(id, request.Title, request.Description, status, request.DueDate);
        if (task == null)
            return NotFound(new { message = "Task not found." });

        var responseDto = MapToDto(task);
        await _hubContext.Clients.All.SendAsync("TaskUpdated", responseDto);
        return Ok(responseDto);
    }

    /// <summary>
    /// Delete a task (soft delete). Regular users can only delete their own tasks.
    /// </summary>
    [HttpDelete("{id}")]
    [PermissionGuard("DELETE:TASK", "DELETE_OTHERS:TASK")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        // Check ownership for regular users
        if (!IsCurrentUserAdmin())
        {
            var isOwner = await _taskService.IsTaskOwnerAsync(id, GetCurrentUserId());
            if (!isOwner)
                return Forbid();
        }

        var result = await _taskService.DeleteTaskAsync(id);
        if (!result)
            return NotFound(new { message = "Task not found." });

        await _hubContext.Clients.All.SendAsync("TaskDeleted", new { id });
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private bool IsCurrentUserAdmin()
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "user_role")?.Value;
        return string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private static TaskResponseDTO MapToDto(Models.TaskItem task)
    {
        var ownerName = task.Owner != null
            ? $"{task.Owner.FirstName} {task.Owner.LastName}".Trim()
            : "Unknown";

        return new TaskResponseDTO(
            task.Id,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.DueDate,
            task.OwnerId,
            ownerName,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}