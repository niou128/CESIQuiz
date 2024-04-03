using Microsoft.Data.Sqlite;
using Quiz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz.Data
{
    public class SQLiteDatabaseService : IDatabaseService
    {
        private readonly string _dbPath;

        public SQLiteDatabaseService(string dbPath)
        {
            _dbPath = dbPath;
        }

        public async Task InitializeDatabaseAsync()
        {
            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Questions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT,
                    Text TEXT,
                    Choices TEXT,
                    CorrectAnswerIndex INTEGER
                );
                ";
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var categories = new List<string>();
            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT DISTINCT Category FROM Questions";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(reader.GetString(0));
                    }
                }
            }
            categories.Insert(0, "Toutes les catégories"); // Ajoutez l'option "Toutes les catégories" en premier
            return categories;
        }


        public Task AddQuestionAsync(Question question)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Question>> GetQuestionsAsync()
        {
            var questions = new List<Question>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Category, Text, Choices, CorrectAnswerIndex FROM Questions";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var question = new Question
                        {
                            Id = reader.GetInt32(0),
                            Category = reader.GetString(1),
                            Text = reader.GetString(2),
                            Choices = DeserializeChoices(reader.GetString(3)),
                            CorrectAnswerIndex = reader.GetInt32(4)
                        };

                        questions.Add(question);
                    }
                }
            }

            return questions;
        }

        private List<string> DeserializeChoices(string serializedChoices)
        {
            // Assumption here is that Choices are stored as a JSON string in the database
            // You'll need to use System.Text.Json or Newtonsoft.Json to deserialize the string
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(serializedChoices);
            }
            catch
            {
                // Handle or log error as appropriate for your application
                return new List<string>();
            }
        }

    }
}
