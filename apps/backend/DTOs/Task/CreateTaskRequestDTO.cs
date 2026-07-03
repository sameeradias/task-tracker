using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Task;

public record CreateTaskRequestDTO(
    [Required] [StringLength(200)] string Title,
    [StringLength(2000)] string? Description,
    string? Status,
    DateTime? DueDate
);