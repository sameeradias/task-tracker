namespace backend.DTOs.Task;

public record TaskFilterRequestDTO(
    string? Status,
    int? OwnerId,
    int Page = 1,
    int PageSize = 10
);