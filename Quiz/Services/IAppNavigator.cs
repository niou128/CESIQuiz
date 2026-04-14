using Quiz.Models;

namespace Quiz.Services;

public interface IAppNavigator
{
    Task ShowLoginAsync();
    Task ShowHomeAsync();
    Task NavigateToQuizAsync(QuizSourceMode mode, int questionCount);
    Task NavigateToScoresAsync();
    Task NavigateToAdminAsync();
    Task GoBackAsync();
}
