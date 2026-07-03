namespace backend.DTOs.Auth;

public record AuthResponseDTO(
    string Token,
    int UserId,
    string Email,
    string FirstName,
    string? LastName,
    string? Role,
    DateTime ExpiresAt
);