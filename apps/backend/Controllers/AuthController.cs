using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Auth;
using backend.Services.AuthService;

namespace backend.Controllers;

[Route("api/auth")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register(RegisterRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _authService.RegisterAsync(request.FirstName, request.LastName ?? "", request.Email, request.Password);
            var token = await _authService.GenerateTokenAsync(user);

            var roleName = user.UserRoles?.FirstOrDefault()?.Role?.Name;
            var expirationMinutes = 480; // from config

            return CreatedAtAction(nameof(Register), new AuthResponseDTO(
                token,
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                roleName,
                DateTime.UtcNow.AddMinutes(expirationMinutes)
            ));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var token = await _authService.GenerateTokenAsync(user);
        var roleName = user.UserRoles?.FirstOrDefault()?.Role?.Name;
        var expirationMinutes = 480;

        return Ok(new AuthResponseDTO(
            token,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            roleName,
            DateTime.UtcNow.AddMinutes(expirationMinutes)
        ));
    }
}