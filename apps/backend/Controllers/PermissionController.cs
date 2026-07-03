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