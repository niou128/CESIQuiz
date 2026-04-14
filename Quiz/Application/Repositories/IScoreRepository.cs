using Quiz.Models;

namespace Quiz.Application.Repositories;

public interface IScoreRepository
{
    Task AddAsync(QuizScore score, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuizScore>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
