using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public record LoginRequestDTO(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);