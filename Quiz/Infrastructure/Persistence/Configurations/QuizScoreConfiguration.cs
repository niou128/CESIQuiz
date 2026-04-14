using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quiz.Models;

namespace Quiz.Infrastructure.Persistence.Configurations;

public sealed class QuizScoreConfiguration : IEntityTypeConfiguration<QuizScore>
{
    public void Configure(EntityTypeBuilder<QuizScore> builder)
    {
        builder.ToTable("Scores");
        builder.HasKey(score => score.Id);
        builder.Property(score => score.CompletedAtUtc).IsRequired();
        builder.Property(score => score.QuestionCount).IsRequired();
        builder.Property(score => score.CorrectAnswers).IsRequired();
        builder.Property(score => score.QuizMode).HasConversion<string>().IsRequired();

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(score => score.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
