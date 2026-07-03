using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using backend.Models;
using backend.Services.UserService;
using backend.Services.PermissionService;

namespace backend.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IPermissionService _permissionService;
    private readonly ApplicationDbContext _context;

    public AuthService(
        IUserService userService, 
        IConfiguration configuration, 
        IPermissionService permissionService,
        ApplicationDbContext context)
    {
        _userService = userService;
        _configuration = configuration;
        _permissionService = permissionService;
        _context = context;
    }

    public async Task<User> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        return await _userService.CreateUserAsync(firstName, lastName, email, password);
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        // Get user with password hash directly from context for security
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        // Check if user is active
        if (!user.IsActive)
            return null;

        return user;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var jwtExpirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        if (string.IsNullOrEmpty(jwtSecret))
            throw new InvalidOperationException("JWT Secret is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Get user permissions
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        var permissionsString = string.Join(",", permissions);

        // Get user role name
        var userRole = user.UserRoles.FirstOrDefault()?.Role;
        var roleName = userRole?.Name ?? "User";

        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("user_role", roleName),
            new Claim("permissions", permissionsString),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}