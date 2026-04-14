using Quiz.Data;
using Quiz.Models;

namespace Quiz.Services;

public sealed class AuthService : IAuthService
{
    private readonly IDatabaseService _databaseService;
    private readonly ISessionService _sessionService;

    public AuthService(IDatabaseService databaseService, ISessionService sessionService)
    {
        _databaseService = databaseService;
        _sessionService = sessionService;
    }

    public UserAccount? CurrentUser => _sessionService.CurrentUser;

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Le nom d'utilisateur et le mot de passe sont obligatoires.");
        }

        var user = await _databaseService.GetUserByUsernameAsync(username.Trim());
        if (user is null || !PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            return (false, "Identifiants invalides.");
        }

        _sessionService.SetCurrentUser(user);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Le nom d'utilisateur et le mot de passe sont obligatoires.");
        }

        if (password.Length < 6)
        {
            return (false, "Le mot de passe doit contenir au moins 6 caractères.");
        }

        var existingUser = await _databaseService.GetUserByUsernameAsync(username.Trim());
        if (existingUser is not null)
        {
            return (false, "Ce nom d'utilisateur existe déjà.");
        }

        var hashedPassword = PasswordHasher.HashPassword(password);
        var user = await _databaseService.CreateUserAsync(username.Trim(), hashedPassword.Hash, hashedPassword.Salt, false);
        _sessionService.SetCurrentUser(user);
        return (true, null);
    }

    public void Logout()
    {
        _sessionService.SetCurrentUser(null);
    }
}
