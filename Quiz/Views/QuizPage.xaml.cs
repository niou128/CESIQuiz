using Quiz.ViewModels;
using Quiz.Models;

namespace Quiz.Views;

public partial class QuizPage : ContentPage
{
    private readonly QuizViewModel _viewModel;

    public QuizPage(QuizViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public Task InitializeAsync(QuizSourceMode mode, int questionCount)
    {
        return _viewModel.LoadQuizAsync(mode, questionCount);
    }
}
