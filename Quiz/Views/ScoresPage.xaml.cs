using Quiz.ViewModels;

namespace Quiz.Views;

public partial class ScoresPage : ContentPage
{
    private readonly ScoresViewModel _viewModel;

    public ScoresPage(ScoresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
