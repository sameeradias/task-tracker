namespace backend.DTOs.User;

public record UserResponseDTO(
    int Id,
    string FirstName,
    string? LastName,
    string Email,
    bool IsActive,
    string? RoleName
);