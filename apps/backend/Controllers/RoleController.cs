using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services.RoleService;
using backend.Services.PermissionService;
using backend.DTOs;
using backend.Attributes;

namespace backend.Controllers;

[Route("api/roles")]
[ApiController]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;

    public RoleController(IRoleService roleService, IPermissionService permissionService)
    {
        _roleService = roleService;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [PermissionGuard("READ:ROLE")]
    public async Task<ActionResult<IEnumerable<RoleResponseDTO>>> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        var response = new List<RoleResponseDTO>();

        foreach (var role in roles)
        {
            var permissions = await _roleService.GetRolePermissionsAsync(role.Id);
            var permissionDtos = permissions.Select(p => new PermissionResponseDTO(
                p.Id,
                p.Name,
                p.Description,
                p.Category
            )).ToList();

            response.Add(new RoleResponseDTO(
                role.Id,
                role.Name,
                role.Description,
                role.IsActive,
                permissionDtos
            ));
        }

        return Ok(response);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [PermissionGuard("READ:ROLE")]
    public async Task<ActionResult<RoleResponseDTO>> GetRoleById(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound(new { message = "Role not found." });
        }

        var permissions = await _roleService.GetRolePermissionsAsync(role.Id);
        var permissionDtos = permissions.Select(p => new PermissionResponseDTO(
            p.Id,
            p.Name,
            p.Description,
            p.Category
        )).ToList();

        var response = new RoleResponseDTO(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            permissionDtos
        );

        return Ok(response);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [PermissionGuard("CREATE:ROLE")]
    public async Task<ActionResult<RoleResponseDTO>> CreateRole(CreateRoleRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = await _roleService.CreateRoleAsync(request.Name, request.Description);
        
        var response = new RoleResponseDTO(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            new List<PermissionResponseDTO>()
        );

        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, response);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    [PermissionGuard("EDIT:ROLE")]
    public async Task<ActionResult<RoleResponseDTO>> UpdateRole(int id, UpdateRoleRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = await _roleService.UpdateRoleAsync(id, request.Name, request.Description);
        if (role == null)
        {
            return NotFound(new { message = "Role not found." });
        }

        var permissions = await _roleService.GetRolePermissionsAsync(role.Id);
        var permissionDtos = permissions.Select(p => new PermissionResponseDTO(
            p.Id,
            p.Name,
            p.Description,
            p.Category
        )).ToList();

        var response = new RoleResponseDTO(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            permissionDtos
        );

        return Ok(response);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    [PermissionGuard("DELETE:ROLE")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (!result)
        {
            return NotFound(new { message = "Role not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    [HttpPost("{roleId}/permissions")]
    [PermissionGuard("ASSIGN:ROLE")]
    public async Task<ActionResult> AssignPermissions(int roleId, AssignPermissionsRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound(new { message = "Role not found." });
        }

        var result = await _roleService.AssignPermissionsToRoleAsync(roleId, request.PermissionIds);
        if (!result)
        {
            return BadRequest(new { message = "Failed to assign permissions to role." });
        }

        return NoContent();
    }

    /// <summary>
    /// Get permissions assigned to a role
    /// </summary>
    [HttpGet("{roleId}/permissions")]
    [PermissionGuard("READ:ROLE")]
    public async Task<ActionResult<IEnumerable<PermissionResponseDTO>>> GetRolePermissions(int roleId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound(new { message = "Role not found." });
        }

        var permissions = await _roleService.GetRolePermissionsAsync(roleId);
        var response = permissions.Select(p => new PermissionResponseDTO(
            p.Id,
            p.Name,
            p.Description,
            p.Category
        ));

        return Ok(response);
    }
}