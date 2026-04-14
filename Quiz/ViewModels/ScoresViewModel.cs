using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quiz.Application.Repositories;
using Quiz.Models;
using Quiz.Services;
using System.Collections.ObjectModel;

namespace Quiz.ViewModels;

public partial class ScoresViewModel : ObservableObject
{
    private readonly IScoreRepository _scoreRepository;
    private readonly ISessionService _sessionService;
    private readonly IPdfExportService _pdfExportService;
    private readonly IAppNavigator _navigator;

    public ScoresViewModel(
        IScoreRepository scoreRepository,
        ISessionService sessionService,
        IPdfExportService pdfExportService,
        IAppNavigator navigator)
    {
        _scoreRepository = scoreRepository;
        _sessionService = sessionService;
        _pdfExportService = pdfExportService;
        _navigator = navigator;
    }

    [ObservableProperty]
    public partial ObservableCollection<QuizScore> Scores { get; set; } = new();

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasScores { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasStatusMessage { get; set; }

    public async Task LoadAsync()
    {
        var currentUser = _sessionService.CurrentUser;
        if (currentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        var items = await _scoreRepository.GetByUserIdAsync(currentUser.Id);
        Scores = new ObservableCollection<QuizScore>(items);
        HasScores = Scores.Count > 0;

        Summary = HasScores
            ? $"Meilleur score : {Scores.Max(score => score.Percentage)} %"
            : "Aucun score enregistré pour le moment.";
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        var currentUser = _sessionService.CurrentUser;
        if (currentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        try
        {
            var exportPath = await _pdfExportService.ExportScoresAsync(currentUser, Scores.ToList());
            StatusMessage = $"Export PDF créé : {exportPath}";
            HasStatusMessage = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            HasStatusMessage = true;
        }
    }
}
