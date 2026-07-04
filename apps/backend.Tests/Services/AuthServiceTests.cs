using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Configuration;
using backend.Models;
using backend.Services.AuthService;
using backend.Services.UserService;
using backend.Services.PermissionService;
using backend.Tests.Helpers;

namespace backend.Tests.Services;

public class AuthServiceTests
{
    private IConfiguration GetTestConfig()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "TestSecretKeyThatIsAtLeast32CharactersLong!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpirationMinutes", "60" }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Register_ShouldCreateNewUser()
    {
        var context = TestDbContextFactory.Create();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var result = await authService.RegisterAsync("John", "Doe", "john@test.com", "Password@123");

        result.Should().NotBeNull();
        result.Email.Should().Be("john@test.com");
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailExists()
    {
        var (context, _) = TestDbContextFactory.CreateWithTestUser();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var act = async () => await authService.RegisterAsync("Test", "User", "test@example.com", "Password@123");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ValidateCredentials_ShouldReturnUser_WhenValid()
    {
        var (context, _) = TestDbContextFactory.CreateWithTestUser();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var result = await authService.ValidateCredentialsAsync("test@example.com", "Test@123");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task ValidateCredentials_ShouldReturnNull_WhenInvalidPassword()
    {
        var (context, _) = TestDbContextFactory.CreateWithTestUser();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var result = await authService.ValidateCredentialsAsync("test@example.com", "WrongPassword");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentials_ShouldReturnNull_WhenEmailNotFound()
    {
        var context = TestDbContextFactory.Create();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var result = await authService.ValidateCredentialsAsync("nobody@test.com", "Password");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnValidJwt()
    {
        var (context, userId, _) = TestDbContextFactory.CreateWithTestUserAndRole();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var user = await userService.GetUserByIdAsync(userId);
        var token = await authService.GenerateTokenAsync(user!);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Length.Should().Be(3); // JWT has 3 parts
    }

    [Fact]
    public async Task GenerateToken_ShouldIncludeUserClaims()
    {
        var (context, userId, _) = TestDbContextFactory.CreateWithTestUserAndRole();
        var userService = new UserService(context);
        var permissionService = new PermissionService(context);
        var config = GetTestConfig();
        var authService = new AuthService(userService, config, permissionService, context);

        var user = await userService.GetUserByIdAsync(userId);
        var token = await authService.GenerateTokenAsync(user!);

        // Decode JWT payload
        var payload = token.Split('.')[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));

        json.Should().Contain("user_id");
        json.Should().Contain("email");
        json.Should().Contain("user_role");
    }
}