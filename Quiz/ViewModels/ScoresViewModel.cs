using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Quiz.Data;
using Quiz.Models;
using Quiz.Services;

namespace Quiz.ViewModels;

public partial class ScoresViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly ISessionService _sessionService;
    private readonly IPdfExportService _pdfExportService;
    private readonly IAppNavigator _navigator;

    public ScoresViewModel(
        IDatabaseService databaseService,
        ISessionService sessionService,
        IPdfExportService pdfExportService,
        IAppNavigator navigator)
    {
        _databaseService = databaseService;
        _sessionService = sessionService;
        _pdfExportService = pdfExportService;
        _navigator = navigator;
    }

    [ObservableProperty]
    private ObservableCollection<QuizScore> scores = new();

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    private bool hasScores;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool hasStatusMessage;

    public async Task LoadAsync()
    {
        var currentUser = _sessionService.CurrentUser;
        if (currentUser is null)
        {
            await _navigator.ShowLoginAsync();
            return;
        }

        var items = await _databaseService.GetScoresForUserAsync(currentUser.Id);
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
