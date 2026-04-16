using Quiz.Models;

namespace Quiz.Services;

public interface IPdfExportService
{
    Task<string> ExportScoresAsync(UserAccount user, IReadOnlyList<QuizScore> scores);
}
