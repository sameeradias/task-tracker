using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.User;
using backend.Services.UserService;
using backend.Attributes;

namespace backend.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [PermissionGuard("READ_OTHERS:USER")]
    public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var response = users.Select(MapToDto);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [PermissionGuard("READ:USER", "READ_OTHERS:USER")]
    public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found." });
        return Ok(MapToDto(user));
    }

    [HttpPost]
    [PermissionGuard("CREATE:USER")]
    public async Task<ActionResult<UserResponseDTO>> CreateUser(CreateUserRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateUserAsync(request.FirstName, request.LastName ?? "", request.Email, request.Password);
            
            if (request.RoleId.HasValue)
                await _userService.AssignRoleToUserAsync(user.Id, request.RoleId.Value);

            user = await _userService.GetUserByIdAsync(user.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = user!.Id }, MapToDto(user));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [PermissionGuard("EDIT:USER", "EDIT_OTHERS:USER")]
    public async Task<ActionResult<UserResponseDTO>> UpdateUser(int id, UpdateUserRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.UpdateUserAsync(id, request.FirstName, request.LastName, request.Email);
        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(MapToDto(user));
    }

    [HttpDelete("{id}")]
    [PermissionGuard("DELETE:USER", "DELETE_OTHERS:USER")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound(new { message = "User not found." });
        return NoContent();
    }

    [HttpPost("{userId}/role")]
    [PermissionGuard("ASSIGN:ROLE")]
    public async Task<ActionResult> AssignRole(int userId, AssignUserRoleRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.AssignRoleToUserAsync(userId, request.RoleId);
        if (!result)
            return NotFound(new { message = "User or role not found." });
        return NoContent();
    }

    [HttpDelete("{userId}/role")]
    [PermissionGuard("ASSIGN:ROLE")]
    public async Task<ActionResult> RemoveRole(int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        var role = await _userService.GetUserRoleAsync(userId);
        if (role == null)
            return NotFound(new { message = "User has no role assigned." });

        var result = await _userService.RemoveRoleFromUserAsync(userId, role.Id);
        if (!result)
            return BadRequest(new { message = "Failed to remove role." });
        return NoContent();
    }

    [HttpGet("{userId}/role")]
    [PermissionGuard("READ:USER", "READ_OTHERS:USER")]
    public async Task<ActionResult> GetUserRole(int userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        var role = await _userService.GetUserRoleAsync(userId);
        if (role == null)
            return Ok(new { message = "No role assigned." });

        return Ok(new { roleId = role.Id, roleName = role.Name });
    }

    private static UserResponseDTO MapToDto(Models.User user)
    {
        var roleName = user.UserRoles?.FirstOrDefault()?.Role?.Name;
        return new UserResponseDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.IsActive,
            roleName
        );
    }
}