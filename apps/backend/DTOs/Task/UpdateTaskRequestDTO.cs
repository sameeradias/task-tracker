using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Task;

public record UpdateTaskRequestDTO(
    [StringLength(200)] string? Title,
    [StringLength(2000)] string? Description,
    string? Status,
    DateTime? DueDate
);