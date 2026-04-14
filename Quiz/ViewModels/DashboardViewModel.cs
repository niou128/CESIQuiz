using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quiz.Models;
using Quiz.Services;

namespace Quiz.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;

    public DashboardViewModel(IAuthService authService, IAppNavigator navigator)
    {
        _authService = authService;
        _navigator = navigator;
    }

    public IReadOnlyList<string> AvailableModes { get; } = new[] { "Local", "Distant" };

    [ObservableProperty]
    public partial string SelectedMode { get; set; } = "Local";

    [ObservableProperty]
    public partial double QuestionCount { get; set; } = 10;

    [ObservableProperty]
    public partial string WelcomeMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanAccessAdmin { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasStatusMessage { get; set; }

    public string QuestionCountLabel => $"Nombre de questions : {(int)QuestionCount}";

    partial void OnQuestionCountChanged(double value)
    {
        OnPropertyChanged(nameof(QuestionCountLabel));
    }

    public Task LoadAsync()
    {
        if (_authService.CurrentUser is null)
        {
            return _navigator.ShowLoginAsync();
        }

        WelcomeMessage = $"Bonjour {_authService.CurrentUser.Username}";
        CanAccessAdmin = _authService.CurrentUser.IsAdmin;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartQuizAsync()
    {
        HasStatusMessage = false;

        if (_authService.CurrentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        var mode = SelectedMode == "Distant" ? QuizSourceMode.Remote : QuizSourceMode.Local;
        await _navigator.NavigateToQuizAsync(mode, (int)QuestionCount);
    }

    [RelayCommand]
    private Task OpenScoresAsync()
    {
        return _navigator.NavigateToScoresAsync();
    }

    [RelayCommand]
    private async Task OpenAdminAsync()
    {
        if (_authService.CurrentUser is null || !_authService.CurrentUser.IsAdmin)
        {
            StatusMessage = "Cette page est réservée au compte administrateur.";
            HasStatusMessage = true;
            return;
        }

        await _navigator.NavigateToAdminAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _authService.Logout();
        await _navigator.ShowLoginAsync();
    }
}
