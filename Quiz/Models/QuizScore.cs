namespace Quiz.Models;

public sealed class QuizScore
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public QuizSourceMode QuizMode { get; set; }
    public int QuestionCount { get; set; }
    public int CorrectAnswers { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public int Percentage => QuestionCount == 0 ? 0 : (int)Math.Round((double)CorrectAnswers / QuestionCount * 100);
    public string ModeLabel => QuizMode == QuizSourceMode.Local ? "Local" : "Distant";
}
