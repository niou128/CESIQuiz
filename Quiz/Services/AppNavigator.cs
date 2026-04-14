using Microsoft.Extensions.DependencyInjection;
using Quiz.Models;
using Quiz.Views;

namespace Quiz.Services;

public sealed class AppNavigator : IAppNavigator
{
    private readonly IServiceProvider _serviceProvider;

    public AppNavigator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task ShowLoginAsync()
    {
        return SetRootAsync<LoginPage>();
    }

    public Task ShowHomeAsync()
    {
        return SetRootAsync<global::Quiz.MainPage>();
    }

    public async Task NavigateToQuizAsync(QuizSourceMode mode, int questionCount)
    {
        var page = _serviceProvider.GetRequiredService<QuizPage>();
        await page.InitializeAsync(mode, questionCount);
        await CurrentNavigation.PushAsync(page);
    }

    public async Task NavigateToScoresAsync()
    {
        var page = _serviceProvider.GetRequiredService<ScoresPage>();
        await CurrentNavigation.PushAsync(page);
    }

    public async Task NavigateToAdminAsync()
    {
        var page = _serviceProvider.GetRequiredService<AdminPage>();
        await CurrentNavigation.PushAsync(page);
    }

    public Task GoBackAsync()
    {
        return CurrentNavigation.PopAsync();
    }

    private Task SetRootAsync<TPage>()
        where TPage : Page
    {
        var page = _serviceProvider.GetRequiredService<TPage>();
        Application.Current!.Windows[0].Page = new NavigationPage(page);
        return Task.CompletedTask;
    }

    private INavigation CurrentNavigation =>
        Application.Current?.Windows[0].Page?.Navigation
        ?? throw new InvalidOperationException("La navigation n'est pas disponible.");
}
