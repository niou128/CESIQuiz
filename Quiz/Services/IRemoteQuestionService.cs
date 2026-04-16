using Quiz.Models;

namespace Quiz.Services;

public interface IRemoteQuestionService
{
    Task<IReadOnlyList<Question>> GetQuestionsAsync(int count, CancellationToken cancellationToken = default);
}
