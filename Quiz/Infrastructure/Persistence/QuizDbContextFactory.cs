using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Quiz.Infrastructure.Persistence;

public sealed class QuizDbContextFactory : IDesignTimeDbContextFactory<QuizDbContext>
{
    public QuizDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QuizDbContext>();
        var databasePath = Path.Combine(AppContext.BaseDirectory, "quiz.design.db");
        optionsBuilder.UseSqlite($"Data Source={databasePath}");
        return new QuizDbContext(optionsBuilder.Options);
    }
}
