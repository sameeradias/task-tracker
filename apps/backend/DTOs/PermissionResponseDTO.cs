namespace backend.DTOs;

public record PermissionResponseDTO(
    int Id,
    string Name,
    string? Description,
    string Category
);