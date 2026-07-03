namespace backend.DTOs;

public record RoleResponseDTO(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    List<PermissionResponseDTO> Permissions
);