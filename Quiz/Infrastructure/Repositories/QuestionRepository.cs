using Microsoft.EntityFrameworkCore;
using Quiz.Application.Repositories;
using Quiz.Infrastructure.Persistence;
using Quiz.Models;

namespace Quiz.Infrastructure.Repositories;

public sealed class QuestionRepository : IQuestionRepository
{
    private readonly IDbContextFactory<QuizDbContext> _dbContextFactory;

    public QuestionRepository(IDbContextFactory<QuizDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<Question>> GetRandomAsync(int count, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Questions
            .FromSqlInterpolated($"""
                SELECT Id, Category, Text, Choices, CorrectAnswerIndex
                FROM Questions
                ORDER BY RANDOM()
                LIMIT {count}
                """)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Questions
            .AsNoTracking()
            .OrderBy(question => question.Category)
            .ThenBy(question => question.Text)
            .ToListAsync(cancellationToken);
    }

    public async Task<Question> SaveAsync(Question question, CancellationToken cancellationToken = default)
    {
        Validate(question);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (question.Id == 0)
        {
            dbContext.Questions.Add(question);
        }
        else
        {
            dbContext.Questions.Update(question);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return question;
    }

    public async Task DeleteAsync(int questionId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Questions.FindAsync(new object[] { questionId }, cancellationToken);
        if (entity is null)
        {
            return;
        }

        dbContext.Questions.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ImportAsync(IEnumerable<Question> questions, CancellationToken cancellationToken = default)
    {
        var items = questions.Select(CloneAndValidate).ToList();
        if (items.Count == 0)
        {
            throw new InvalidOperationException("Le fichier JSON ne contient aucune question.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Questions.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Question CloneAndValidate(Question question)
    {
        var clone = new Question
        {
            Category = question.Category,
            Text = question.Text,
            Choices = question.Choices.ToList(),
            CorrectAnswerIndex = question.CorrectAnswerIndex
        };

        Validate(clone);
        return clone;
    }

    private static void Validate(Question question)
    {
        if (string.IsNullOrWhiteSpace(question.Category))
        {
            throw new InvalidOperationException("La catégorie est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(question.Text))
        {
            throw new InvalidOperationException("L'intitulé de la question est obligatoire.");
        }

        if (question.Choices.Count != 4 || question.Choices.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("Chaque question doit contenir exactement 4 réponses.");
        }

        if (question.CorrectAnswerIndex < 0 || question.CorrectAnswerIndex > 3)
        {
            throw new InvalidOperationException("L'index de la bonne réponse doit être compris entre 0 et 3.");
        }
    }
}
