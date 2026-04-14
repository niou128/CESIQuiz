using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Quiz.Models;

public partial class AnswerOption : ObservableObject
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private bool isCorrect;

    [ObservableProperty]
    private bool isEnabled = true;

    [ObservableProperty]
    private Color backgroundColor = Color.FromArgb("#1565C0");
}
