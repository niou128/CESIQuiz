using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quiz.Models;

namespace Quiz.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Question> builder)
    {
        var choicesComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            choices => choices.Aggregate(0, (current, value) => HashCode.Combine(current, value.GetHashCode())),
            choices => choices.ToList());

        builder.ToTable("Questions");
        builder.HasKey(question => question.Id);
        builder.Property(question => question.Category).IsRequired();
        builder.Property(question => question.Text).IsRequired();
        builder.Property(question => question.CorrectAnswerIndex).IsRequired();
        builder.Property(question => question.Choices)
            .HasConversion(
                choices => JsonSerializer.Serialize(choices, JsonOptions),
                serialized => JsonSerializer.Deserialize<List<string>>(serialized, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(choicesComparer);
    }
}
