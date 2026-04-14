using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Quiz.Application.Repositories;
using Quiz.Infrastructure.Persistence;
using Quiz.Models;

namespace Quiz.Services;

public sealed class AppInitializer : IAppInitializer
{
    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminPassword = "Admin123!";

    private readonly IDbContextFactory<QuizDbContext> _dbContextFactory;
    private readonly IUserRepository _userRepository;

    public AppInitializer(IDbContextFactory<QuizDbContext> dbContextFactory, IUserRepository userRepository)
    {
        _dbContextFactory = dbContextFactory;
        _userRepository = userRepository;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!await dbContext.Questions.AnyAsync(cancellationToken))
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            await using var stream = assembly.GetManifestResourceStream("Quiz.sql.InitializeDatabase.sql");
            if (stream is null)
            {
                throw new FileNotFoundException("Embedded SQL resource not found.", "Quiz.sql.InitializeDatabase.sql");
            }

            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync(cancellationToken);
            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }

        var adminUser = await _userRepository.GetByUsernameAsync(DefaultAdminUserName, cancellationToken);
        if (adminUser is null)
        {
            var hashedPassword = PasswordHasher.HashPassword(DefaultAdminPassword);
            await _userRepository.AddAsync(new UserAccount
            {
                Username = DefaultAdminUserName,
                PasswordHash = hashedPassword.Hash,
                PasswordSalt = hashedPassword.Salt,
                IsAdmin = true,
                CreatedAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
