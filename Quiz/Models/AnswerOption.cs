using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Quiz.Models;

public partial class AnswerOption : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsCorrect { get; set; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial Color BackgroundColor { get; set; } = Color.FromArgb("#1565C0");
}
