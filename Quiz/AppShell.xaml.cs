using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Quiz.Services;
using Quiz.Views;
using ShellFlyoutBehavior = Microsoft.Maui.FlyoutBehavior;

namespace Quiz;

public partial class AppShell : Shell
{
    public const string HomeRoute = "home";
    public const string ScoresRoute = "scores";
    public const string AdminRoute = "admin";

    private readonly IServiceProvider _serviceProvider;
    private readonly ISessionService _sessionService;
    private FlyoutItem? _adminItem;

    public AppShell(IServiceProvider serviceProvider, ISessionService sessionService)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
        _sessionService = sessionService;

        FlyoutBehavior = ShellFlyoutBehavior.Flyout;

        Items.Add(CreateFlyoutItem("Accueil", HomeRoute, () => _serviceProvider.GetRequiredService<MainPage>()));
        Items.Add(CreateFlyoutItem("Mes scores", ScoresRoute, () => _serviceProvider.GetRequiredService<ScoresPage>()));
        _adminItem = CreateFlyoutItem("Administration", AdminRoute, () => _serviceProvider.GetRequiredService<AdminPage>());
        Items.Add(_adminItem);

        UpdateAdminVisibility();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateAdminVisibility();
    }

    private FlyoutItem CreateFlyoutItem(string title, string route, Func<Page> pageFactory)
    {
        return new FlyoutItem
        {
            Title = title,
            Route = route,
            Items =
            {
                new ShellContent
                {
                    Title = title,
                    Route = $"{route}-content",
                    ContentTemplate = new DataTemplate(pageFactory)
                }
            }
        };
    }

    private void UpdateAdminVisibility()
    {
        if (_adminItem is not null)
        {
            _adminItem.IsVisible = _sessionService.CurrentUser?.IsAdmin == true;
        }
    }
}
