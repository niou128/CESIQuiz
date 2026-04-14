using System.Reflection;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Quiz.Models;
using Quiz.Services;

namespace Quiz.Data;

public sealed class SQLiteDatabaseService : IDatabaseService
{
    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminPassword = "Admin123!";
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SQLiteDatabaseService(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Questions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Category TEXT NOT NULL,
                Text TEXT NOT NULL,
                Choices TEXT NOT NULL,
                CorrectAnswerIndex INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                IsAdmin INTEGER NOT NULL DEFAULT 0,
                CreatedAtUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Scores (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                QuizMode TEXT NOT NULL,
                QuestionCount INTEGER NOT NULL,
                CorrectAnswers INTEGER NOT NULL,
                CompletedAtUtc TEXT NOT NULL,
                FOREIGN KEY(UserId) REFERENCES Users(Id)
            );
            """;
        await command.ExecuteNonQueryAsync();

        await EnsureDefaultQuestionsAsync(connection);
        await EnsureAdminAccountAsync(connection);
    }

    public async Task<IReadOnlyList<Question>> GetRandomQuestionsAsync(int count)
    {
        var questions = new List<Question>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Category, Text, Choices, CorrectAnswerIndex
            FROM Questions
            ORDER BY RANDOM()
            LIMIT $count;
            """;
        command.Parameters.AddWithValue("$count", count);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            questions.Add(MapQuestion(reader));
        }

        return questions;
    }

    public async Task<IReadOnlyList<Question>> GetAllQuestionsAsync()
    {
        var questions = new List<Question>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Category, Text, Choices, CorrectAnswerIndex
            FROM Questions
            ORDER BY Category, Text;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            questions.Add(MapQuestion(reader));
        }

        return questions;
    }

    public async Task<Question> SaveQuestionAsync(Question question)
    {
        ValidateQuestion(question);

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var serializedChoices = JsonSerializer.Serialize(question.Choices, _jsonOptions);
        var command = connection.CreateCommand();

        if (question.Id == 0)
        {
            command.CommandText =
                """
                INSERT INTO Questions (Category, Text, Choices, CorrectAnswerIndex)
                VALUES ($category, $text, $choices, $correctAnswerIndex);
                SELECT last_insert_rowid();
                """;
        }
        else
        {
            command.CommandText =
                """
                UPDATE Questions
                SET Category = $category,
                    Text = $text,
                    Choices = $choices,
                    CorrectAnswerIndex = $correctAnswerIndex
                WHERE Id = $id;
                SELECT $id;
                """;
            command.Parameters.AddWithValue("$id", question.Id);
        }

        command.Parameters.AddWithValue("$category", question.Category.Trim());
        command.Parameters.AddWithValue("$text", question.Text.Trim());
        command.Parameters.AddWithValue("$choices", serializedChoices);
        command.Parameters.AddWithValue("$correctAnswerIndex", question.CorrectAnswerIndex);

        var savedId = Convert.ToInt32(await command.ExecuteScalarAsync());
        question.Id = savedId;
        return question;
    }

    public async Task DeleteQuestionAsync(int questionId)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Questions WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", questionId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task ImportQuestionsAsync(IEnumerable<Question> questions)
    {
        var sanitizedQuestions = questions
            .Select(question => new Question
            {
                Category = question.Category,
                Text = question.Text,
                Choices = question.Choices,
                CorrectAnswerIndex = question.CorrectAnswerIndex
            })
            .ToList();

        if (sanitizedQuestions.Count == 0)
        {
            throw new InvalidOperationException("Le fichier JSON ne contient aucune question.");
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        foreach (var question in sanitizedQuestions)
        {
            ValidateQuestion(question);

            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                INSERT INTO Questions (Category, Text, Choices, CorrectAnswerIndex)
                VALUES ($category, $text, $choices, $correctAnswerIndex);
                """;
            command.Parameters.AddWithValue("$category", question.Category.Trim());
            command.Parameters.AddWithValue("$text", question.Text.Trim());
            command.Parameters.AddWithValue("$choices", JsonSerializer.Serialize(question.Choices, _jsonOptions));
            command.Parameters.AddWithValue("$correctAnswerIndex", question.CorrectAnswerIndex);
            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task<UserAccount?> GetUserByUsernameAsync(string username)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAtUtc
            FROM Users
            WHERE Username = $username;
            """;
        command.Parameters.AddWithValue("$username", username.Trim());

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<UserAccount> CreateUserAsync(string username, string passwordHash, string passwordSalt, bool isAdmin)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Users (Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAtUtc)
            VALUES ($username, $passwordHash, $passwordSalt, $isAdmin, $createdAtUtc);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$username", username.Trim());
        command.Parameters.AddWithValue("$passwordHash", passwordHash);
        command.Parameters.AddWithValue("$passwordSalt", passwordSalt);
        command.Parameters.AddWithValue("$isAdmin", isAdmin ? 1 : 0);
        command.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.ToString("O"));

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return new UserAccount
        {
            Id = id,
            Username = username.Trim(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsAdmin = isAdmin,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public async Task SaveScoreAsync(QuizScore score)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Scores (UserId, QuizMode, QuestionCount, CorrectAnswers, CompletedAtUtc)
            VALUES ($userId, $quizMode, $questionCount, $correctAnswers, $completedAtUtc);
            """;
        command.Parameters.AddWithValue("$userId", score.UserId);
        command.Parameters.AddWithValue("$quizMode", score.QuizMode.ToString());
        command.Parameters.AddWithValue("$questionCount", score.QuestionCount);
        command.Parameters.AddWithValue("$correctAnswers", score.CorrectAnswers);
        command.Parameters.AddWithValue("$completedAtUtc", score.CompletedAtUtc.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<QuizScore>> GetScoresForUserAsync(int userId)
    {
        var scores = new List<QuizScore>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, UserId, QuizMode, QuestionCount, CorrectAnswers, CompletedAtUtc
            FROM Scores
            WHERE UserId = $userId
            ORDER BY CompletedAtUtc DESC;
            """;
        command.Parameters.AddWithValue("$userId", userId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            scores.Add(new QuizScore
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                QuizMode = Enum.TryParse<QuizSourceMode>(reader.GetString(2), out var mode)
                    ? mode
                    : QuizSourceMode.Local,
                QuestionCount = reader.GetInt32(3),
                CorrectAnswers = reader.GetInt32(4),
                CompletedAtUtc = DateTime.Parse(reader.GetString(5))
            });
        }

        return scores;
    }

    private async Task EnsureDefaultQuestionsAsync(SqliteConnection connection)
    {
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Questions;";
        var existingCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        if (existingCount > 0)
        {
            return;
        }

        var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
        await using var stream = assembly.GetManifestResourceStream("Quiz.sql.InitializeDatabase.sql");
        if (stream is null)
        {
            throw new FileNotFoundException("Embedded SQL resource not found.", "Quiz.sql.InitializeDatabase.sql");
        }

        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync();

        var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private async Task EnsureAdminAccountAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = $username;";
        command.Parameters.AddWithValue("$username", DefaultAdminUserName);
        var existingCount = Convert.ToInt32(await command.ExecuteScalarAsync());
        if (existingCount > 0)
        {
            return;
        }

        var hashedPassword = PasswordHasher.HashPassword(DefaultAdminPassword);
        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText =
            """
            INSERT INTO Users (Username, PasswordHash, PasswordSalt, IsAdmin, CreatedAtUtc)
            VALUES ($username, $passwordHash, $passwordSalt, 1, $createdAtUtc);
            """;
        insertCommand.Parameters.AddWithValue("$username", DefaultAdminUserName);
        insertCommand.Parameters.AddWithValue("$passwordHash", hashedPassword.Hash);
        insertCommand.Parameters.AddWithValue("$passwordSalt", hashedPassword.Salt);
        insertCommand.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.ToString("O"));

        await insertCommand.ExecuteNonQueryAsync();
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath}");
    }

    private Question MapQuestion(SqliteDataReader reader)
    {
        return new Question
        {
            Id = reader.GetInt32(0),
            Category = reader.GetString(1),
            Text = reader.GetString(2),
            Choices = JsonSerializer.Deserialize<List<string>>(reader.GetString(3), _jsonOptions) ?? new List<string>(),
            CorrectAnswerIndex = reader.GetInt32(4)
        };
    }

    private static UserAccount MapUser(SqliteDataReader reader)
    {
        return new UserAccount
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            IsAdmin = reader.GetInt32(4) == 1,
            CreatedAtUtc = DateTime.Parse(reader.GetString(5))
        };
    }

    private static void ValidateQuestion(Question question)
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
