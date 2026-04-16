using Microsoft.EntityFrameworkCore;
using Quiz.Models;

namespace Quiz.Infrastructure.Persistence;

public sealed class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options)
        : base(options)
    {
    }

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<QuizScore> Scores => Set<QuizScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
