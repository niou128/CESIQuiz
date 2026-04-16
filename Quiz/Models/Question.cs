namespace Quiz.Models;

public sealed class Question
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Choices { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
}
