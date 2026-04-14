using Quiz.Models;

namespace Quiz.Services;

public interface ISessionService
{
    UserAccount? CurrentUser { get; }
    void SetCurrentUser(UserAccount? user);
}
