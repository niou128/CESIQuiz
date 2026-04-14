using Quiz.Models;

namespace Quiz.Application.Repositories;

public interface IUserRepository
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserAccount> AddAsync(UserAccount user, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
