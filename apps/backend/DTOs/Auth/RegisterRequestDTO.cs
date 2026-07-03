using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public record RegisterRequestDTO(
    [Required] [StringLength(100)] string FirstName,
    [StringLength(100)] string? LastName,
    [Required] [EmailAddress] [StringLength(255)] string Email,
    [Required] [MinLength(6)] string Password
);