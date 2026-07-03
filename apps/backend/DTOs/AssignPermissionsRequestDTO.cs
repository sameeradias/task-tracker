using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public record AssignPermissionsRequestDTO(
    [Required] List<int> PermissionIds
);