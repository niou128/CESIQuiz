using Microsoft.EntityFrameworkCore;
using Quiz.Application.Repositories;
using Quiz.Infrastructure.Persistence;
using Quiz.Models;

namespace Quiz.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<QuizDbContext> _dbContextFactory;

    public UserRepository(IDbContextFactory<QuizDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);
    }

    public async Task<UserAccount> AddAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Users.AnyAsync(cancellationToken);
    }
}
