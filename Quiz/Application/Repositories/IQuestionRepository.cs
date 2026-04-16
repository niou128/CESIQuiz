using Quiz.Models;

namespace Quiz.Application.Repositories;

public interface IQuestionRepository
{
    Task<IReadOnlyList<Question>> GetRandomAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Question> SaveAsync(Question question, CancellationToken cancellationToken = default);
    Task DeleteAsync(int questionId, CancellationToken cancellationToken = default);
    Task ImportAsync(IEnumerable<Question> questions, CancellationToken cancellationToken = default);
}
