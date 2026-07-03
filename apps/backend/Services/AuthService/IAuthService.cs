using backend.Models;

namespace backend.Services.AuthService;

public interface IAuthService
{
    Task<User> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<User?> ValidateCredentialsAsync(string email, string password);
    Task<string> GenerateTokenAsync(User user);
}