using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using Quiz.Data;
using Quiz.Models;
using Quiz.Services;

namespace Quiz.ViewModels;

public partial class QuizViewModel : ObservableObject
{
    private static readonly string[] DefaultAnswerColors = ["#E53935", "#1E88E5", "#FDD835", "#43A047"];
    private readonly IDatabaseService _databaseService;
    private readonly IRemoteQuestionService _remoteQuestionService;
    private readonly ISessionService _sessionService;
    private readonly IAppNavigator _navigator;
    private readonly List<Question> _questions = new();
    private int _currentIndex;
    private int _correctAnswers;

    [ObservableProperty]
    private Question? currentQuestion;

    [ObservableProperty]
    private ObservableCollection<AnswerOption> answerOptions = new();

    [ObservableProperty]
    private string progressText = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool hasStatusMessage;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool canMoveNext;

    [ObservableProperty]
    private bool hasActiveQuestion;

    public QuizViewModel(
        IDatabaseService databaseService,
        IRemoteQuestionService remoteQuestionService,
        ISessionService sessionService,
        IAppNavigator navigator)
    {
        _databaseService = databaseService;
        _remoteQuestionService = remoteQuestionService;
        _sessionService = sessionService;
        _navigator = navigator;
    }

    public async Task LoadQuizAsync(QuizSourceMode mode, int questionCount)
    {
        if (_sessionService.CurrentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        questionCount = Math.Clamp(questionCount, 5, 20);
        ResetState();
        IsBusy = true;

        try
        {
            IReadOnlyList<Question> fetchedQuestions = mode == QuizSourceMode.Local
                ? await _databaseService.GetRandomQuestionsAsync(questionCount)
                : await _remoteQuestionService.GetQuestionsAsync(questionCount);

            if (fetchedQuestions.Count < 5)
            {
                throw new InvalidOperationException("Le quiz nécessite au moins 5 questions.");
            }

            _questions.AddRange(fetchedQuestions);
            CurrentMode = mode;
            QuestionCount = _questions.Count;
            LoadQuestion(0);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            HasStatusMessage = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public QuizSourceMode CurrentMode { get; private set; } = QuizSourceMode.Local;

    public int QuestionCount { get; private set; }

    [RelayCommand]
    private void SelectAnswer(AnswerOption option)
    {
        if (CurrentQuestion is null || CanMoveNext)
        {
            return;
        }

        foreach (var currentOption in AnswerOptions)
        {
            currentOption.IsEnabled = false;

            if (currentOption.IsCorrect)
            {
                currentOption.BackgroundColor = Color.FromArgb("#2E7D32");
            }
            else if (ReferenceEquals(currentOption, option))
            {
                currentOption.BackgroundColor = Color.FromArgb("#C62828");
            }
            else
            {
                currentOption.BackgroundColor = Color.FromArgb("#90A4AE");
            }
        }

        if (option.IsCorrect)
        {
            _correctAnswers++;
        }

        CanMoveNext = true;
    }

    [RelayCommand]
    private async Task NextQuestionAsync()
    {
        if (_currentIndex < _questions.Count - 1)
        {
            LoadQuestion(_currentIndex + 1);
            return;
        }

        await SaveScoreAndExitAsync();
    }

    private async Task SaveScoreAndExitAsync()
    {
        var currentUser = _sessionService.CurrentUser;
        if (currentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        await _databaseService.SaveScoreAsync(new QuizScore
        {
            UserId = currentUser.Id,
            QuizMode = CurrentMode,
            QuestionCount = QuestionCount,
            CorrectAnswers = _correctAnswers,
            CompletedAtUtc = DateTime.UtcNow
        });

        await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Quiz terminé",
            $"Score : {_correctAnswers}/{QuestionCount}",
            "OK");

        await _navigator.GoBackAsync();
    }

    private void ResetState()
    {
        _questions.Clear();
        AnswerOptions.Clear();
        _currentIndex = 0;
        _correctAnswers = 0;
        CurrentQuestion = null;
        CanMoveNext = false;
        HasActiveQuestion = false;
        StatusMessage = string.Empty;
        HasStatusMessage = false;
        ProgressText = string.Empty;
    }

    private void LoadQuestion(int index)
    {
        _currentIndex = index;
        CurrentQuestion = _questions[index];
        ProgressText = $"Question {index + 1} / {_questions.Count}";
        CanMoveNext = false;
        HasActiveQuestion = true;

        AnswerOptions = new ObservableCollection<AnswerOption>(
            CurrentQuestion.Choices.Select((choice, choiceIndex) => new AnswerOption
            {
                Text = choice,
                IsCorrect = choiceIndex == CurrentQuestion.CorrectAnswerIndex,
                BackgroundColor = Color.FromArgb(DefaultAnswerColors[choiceIndex % DefaultAnswerColors.Length])
            }));
    }
}
