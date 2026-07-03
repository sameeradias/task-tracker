using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public record CreateRoleRequestDTO(
    [Required] [StringLength(100, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description
);