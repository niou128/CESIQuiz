using Quiz.Models;

namespace Quiz.Services;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string username, string password);
    void Logout();
    UserAccount? CurrentUser { get; }
}
