using Microsoft.EntityFrameworkCore;
using Quiz.Application.Repositories;
using Quiz.Infrastructure.Persistence;
using Quiz.Models;

namespace Quiz.Infrastructure.Repositories;

public sealed class ScoreRepository : IScoreRepository
{
    private readonly IDbContextFactory<QuizDbContext> _dbContextFactory;

    public ScoreRepository(IDbContextFactory<QuizDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddAsync(QuizScore score, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Scores.Add(score);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QuizScore>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Scores
            .AsNoTracking()
            .Where(score => score.UserId == userId)
            .OrderByDescending(score => score.CompletedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
