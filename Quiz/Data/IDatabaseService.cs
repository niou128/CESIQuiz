using Quiz.Models;

namespace Quiz.Data;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<IReadOnlyList<Question>> GetRandomQuestionsAsync(int count);
    Task<IReadOnlyList<Question>> GetAllQuestionsAsync();
    Task<Question> SaveQuestionAsync(Question question);
    Task DeleteQuestionAsync(int questionId);
    Task ImportQuestionsAsync(IEnumerable<Question> questions);
    Task<UserAccount?> GetUserByUsernameAsync(string username);
    Task<UserAccount> CreateUserAsync(string username, string passwordHash, string passwordSalt, bool isAdmin);
    Task SaveScoreAsync(QuizScore score);
    Task<IReadOnlyList<QuizScore>> GetScoresForUserAsync(int userId);
}
