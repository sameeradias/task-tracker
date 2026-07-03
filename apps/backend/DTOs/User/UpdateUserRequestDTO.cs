using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User;

public record UpdateUserRequestDTO(
    [StringLength(100)] string? FirstName,
    [StringLength(100)] string? LastName,
    [EmailAddress] [StringLength(255)] string? Email
);