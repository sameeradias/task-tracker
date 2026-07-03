using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public record AssignRoleRequestDTO(
    [Required] int UserId,
    [Required] int RoleId
);