using System.Net.Http.Json;
using Quiz.Models;

namespace Quiz.Services;

public sealed class OpenTriviaDbService : IRemoteQuestionService
{
    private readonly HttpClient _httpClient;

    public OpenTriviaDbService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Question>> GetQuestionsAsync(int count, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<OpenTriviaResponse>(
            $"https://opentdb.com/api.php?amount={count}&type=multiple&encode=url3986",
            cancellationToken);

        if (response is null || response.response_code != 0 || response.results is null || response.results.Count == 0)
        {
            throw new InvalidOperationException("La source distante n'a renvoyé aucune question.");
        }

        return response.results.Select(MapQuestion).ToList();
    }

    private static Question MapQuestion(OpenTriviaQuestionDto dto)
    {
        var decodedCorrectAnswer = Decode(dto.correct_answer);
        var allChoices = dto.incorrect_answers.Select(Decode).Append(decodedCorrectAnswer).ToList();
        Shuffle(allChoices);

        return new Question
        {
            Category = Decode(dto.category),
            Text = Decode(dto.question),
            Choices = allChoices,
            CorrectAnswerIndex = allChoices.IndexOf(decodedCorrectAnswer)
        };
    }

    private static string Decode(string input)
    {
        return Uri.UnescapeDataString(input ?? string.Empty);
    }

    private static void Shuffle(List<string> items)
    {
        for (var index = items.Count - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }

    private sealed class OpenTriviaResponse
    {
        public int response_code { get; set; }
        public List<OpenTriviaQuestionDto>? results { get; set; }
    }

    private sealed class OpenTriviaQuestionDto
    {
        public string category { get; set; } = string.Empty;
        public string question { get; set; } = string.Empty;
        public string correct_answer { get; set; } = string.Empty;
        public List<string> incorrect_answers { get; set; } = new();
    }
}
