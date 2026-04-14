using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
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
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        Microsoft.Maui.Controls.Application.Current!.Windows[0].Page = shell;
        return Task.CompletedTask;
    }

    public async Task NavigateToQuizAsync(QuizSourceMode mode, int questionCount)
    {
        var page = _serviceProvider.GetRequiredService<QuizPage>();
        await page.InitializeAsync(mode, questionCount);
        await Shell.Current.Navigation.PushAsync(page);
    }

    public async Task NavigateToScoresAsync()
    {
        await Shell.Current.GoToAsync($"//{AppShell.ScoresRoute}");
    }

    public async Task NavigateToAdminAsync()
    {
        await Shell.Current.GoToAsync($"//{AppShell.AdminRoute}");
    }

    public Task GoBackAsync()
    {
        if (Shell.Current?.Navigation?.NavigationStack?.Count > 1)
        {
            return Shell.Current.Navigation.PopAsync();
        }

        return Task.CompletedTask;
    }

    private Task SetRootAsync<TPage>()
        where TPage : Page
    {
        var page = _serviceProvider.GetRequiredService<TPage>();
        Microsoft.Maui.Controls.Application.Current!.Windows[0].Page = new NavigationPage(page);
        return Task.CompletedTask;
    }
}
