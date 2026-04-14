using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using Quiz.Data;
using Quiz.Models;
using Quiz.Services;

namespace Quiz.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly ISessionService _sessionService;
    private readonly IAppNavigator _navigator;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AdminViewModel(
        IDatabaseService databaseService,
        ISessionService sessionService,
        IAppNavigator navigator)
    {
        _databaseService = databaseService;
        _sessionService = sessionService;
        _navigator = navigator;
    }

    [ObservableProperty]
    private ObservableCollection<Question> questions = new();

    [ObservableProperty]
    private Question? selectedQuestion;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string questionText = string.Empty;

    [ObservableProperty]
    private string choice1 = string.Empty;

    [ObservableProperty]
    private string choice2 = string.Empty;

    [ObservableProperty]
    private string choice3 = string.Empty;

    [ObservableProperty]
    private string choice4 = string.Empty;

    [ObservableProperty]
    private int correctAnswerIndex;

    [ObservableProperty]
    private bool isAdmin;

    [ObservableProperty]
    private bool hasSelectedQuestion;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool hasStatusMessage;

    public IReadOnlyList<string> CorrectAnswerLabels { get; } = new[] { "Choix 1", "Choix 2", "Choix 3", "Choix 4" };

    partial void OnSelectedQuestionChanged(Question? value)
    {
        HasSelectedQuestion = value is not null;
        if (value is null)
        {
            return;
        }

        Category = value.Category;
        QuestionText = value.Text;
        Choice1 = value.Choices.ElementAtOrDefault(0) ?? string.Empty;
        Choice2 = value.Choices.ElementAtOrDefault(1) ?? string.Empty;
        Choice3 = value.Choices.ElementAtOrDefault(2) ?? string.Empty;
        Choice4 = value.Choices.ElementAtOrDefault(3) ?? string.Empty;
        CorrectAnswerIndex = value.CorrectAnswerIndex;
    }

    public async Task LoadAsync()
    {
        var currentUser = _sessionService.CurrentUser;
        if (currentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        IsAdmin = currentUser.IsAdmin;
        if (!IsAdmin)
        {
            StatusMessage = "Cette page est réservée au compte administrateur.";
            HasStatusMessage = true;
            Questions.Clear();
            return;
        }

        await ReloadQuestionsAsync();
    }

    [RelayCommand]
    private void NewQuestion()
    {
        SelectedQuestion = null;
        Category = string.Empty;
        QuestionText = string.Empty;
        Choice1 = string.Empty;
        Choice2 = string.Empty;
        Choice3 = string.Empty;
        Choice4 = string.Empty;
        CorrectAnswerIndex = 0;
        HasSelectedQuestion = false;
        StatusMessage = string.Empty;
        HasStatusMessage = false;
    }

    [RelayCommand]
    private async Task SaveQuestionAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        try
        {
            var question = new Question
            {
                Id = SelectedQuestion?.Id ?? 0,
                Category = Category,
                Text = QuestionText,
                Choices = new List<string> { Choice1, Choice2, Choice3, Choice4 },
                CorrectAnswerIndex = CorrectAnswerIndex
            };

            await _databaseService.SaveQuestionAsync(question);
            await ReloadQuestionsAsync();
            SelectedQuestion = Questions.FirstOrDefault(item => item.Id == question.Id);
            StatusMessage = "Question enregistrée.";
            HasStatusMessage = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            HasStatusMessage = true;
        }
    }

    [RelayCommand]
    private async Task DeleteQuestionAsync()
    {
        if (!IsAdmin || SelectedQuestion is null)
        {
            return;
        }

        await _databaseService.DeleteQuestionAsync(SelectedQuestion.Id);
        NewQuestion();
        await ReloadQuestionsAsync();
        StatusMessage = "Question supprimée.";
        HasStatusMessage = true;
    }

    [RelayCommand]
    private async Task ImportJsonAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Sélectionner un fichier JSON"
            });

            if (result is null)
            {
                return;
            }

            await using var stream = await result.OpenReadAsync();
            var importedQuestions = await JsonSerializer.DeserializeAsync<List<Question>>(stream, _jsonOptions);
            if (importedQuestions is null)
            {
                throw new InvalidOperationException("Impossible de lire le contenu JSON.");
            }

            await _databaseService.ImportQuestionsAsync(importedQuestions);
            await ReloadQuestionsAsync();
            StatusMessage = $"{importedQuestions.Count} question(s) importée(s).";
            HasStatusMessage = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            HasStatusMessage = true;
        }
    }

    private async Task ReloadQuestionsAsync()
    {
        var items = await _databaseService.GetAllQuestionsAsync();
        Questions = new ObservableCollection<Question>(items);
    }
}
