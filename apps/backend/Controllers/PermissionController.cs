using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services.PermissionService;
using backend.DTOs;
using backend.Attributes;

namespace backend.Controllers;

[Route("api/permissions")]
[ApiController]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all permissions in the system
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionResponseDTO>>> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        var response = permissions.Select(p => new PermissionResponseDTO(
            p.Id,
            p.Name,
            p.Description,
            p.Category
        ));

        return Ok(response);
    }

    /// <summary>
    /// Get permissions assigned to the current user (through their role)
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<PermissionResponseDTO>>> GetMyPermissions()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Super Admin gets all permissions
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "user_role")?.Value;
        if (string.Equals(userRole, "Super Admin", StringComparison.OrdinalIgnoreCase))
        {
            var allPerms = await _permissionService.GetAllPermissionsAsync();
            return Ok(allPerms.Select(p => new PermissionResponseDTO(p.Id, p.Name, p.Description, p.Category)));
        }

        // Regular users get only their role's permissions
        var permissionNames = await _permissionService.GetUserPermissionsAsync(userId);
        var allPermissions = await _permissionService.GetAllPermissionsAsync();
        var userPermissions = allPermissions.Where(p => permissionNames.Contains(p.Name));
        
        return Ok(userPermissions.Select(p => new PermissionResponseDTO(p.Id, p.Name, p.Description, p.Category)));
    }

    /// <summary>
    /// Get permissions by category
    /// </summary>
    [HttpGet("category/{category}")]
    [PermissionGuard("READ:PERMISSION")]
    public async Task<ActionResult<IEnumerable<PermissionResponseDTO>>> GetByCategory(string category)
    {
        var permissions = await _permissionService.GetPermissionsByCategoryAsync(category);
        var response = permissions.Select(p => new PermissionResponseDTO(
            p.Id,
            p.Name,
            p.Description,
            p.Category
        ));

        return Ok(response);
    }
}