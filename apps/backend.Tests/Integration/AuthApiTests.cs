using System.Linq;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;
using Xunit;

namespace backend.Tests.Integration;

public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all EF Core related services
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                    .ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Also remove the specific ApplicationDbContext
                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ApplicationDbContext));
                if (contextDescriptor != null) services.Remove(contextDescriptor);

                // Add InMemory database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenInvalidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@test.com",
            password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ShouldReturn201_WhenValid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "New",
            lastName = "User",
            email = $"newuser_{Guid.NewGuid()}@test.com",
            password = "Password@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ShouldReturnToken_InResponse()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Token",
            lastName = "User",
            email = $"tokenuser_{Guid.NewGuid()}@test.com",
            password = "Password@123"
        });

        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        content.Should().ContainKey("token");
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailExists()
    {
        var email = $"duplicate_{Guid.NewGuid()}@test.com";

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "First",
            lastName = "User",
            email = email,
            password = "Password@123"
        });

        // Duplicate registration
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Second",
            lastName = "User",
            email = email,
            password = "Password@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}