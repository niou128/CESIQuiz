using Quiz.Models;

namespace Quiz.Services;

public sealed class SessionService : ISessionService
{
    public UserAccount? CurrentUser { get; private set; }

    public void SetCurrentUser(UserAccount? user)
    {
        CurrentUser = user;
    }
}
