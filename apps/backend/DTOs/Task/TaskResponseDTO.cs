namespace backend.DTOs.Task;

public record TaskResponseDTO(
    int Id,
    string Title,
    string? Description,
    string Status,
    DateTime? DueDate,
    int OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);