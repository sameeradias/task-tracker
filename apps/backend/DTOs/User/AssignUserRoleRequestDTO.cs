using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User;

public record AssignUserRoleRequestDTO(
    [Required] int RoleId
);