using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Quiz.Data;
using Quiz.Views;

namespace Quiz;

public partial class App : Application
{
    private Page _startupPage;

    public App(IDatabaseService databaseService, LoginPage loginPage)
    {
        InitializeComponent();

        _startupPage = loginPage;

        try
        {
            databaseService.InitializeAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Startup failure: {ex}");
            _startupPage = BuildErrorPage(ex);
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(_startupPage));
    }

    private static Page BuildErrorPage(Exception exception)
    {
        return new ContentPage
        {
            Title = "Erreur",
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 24,
                    Spacing = 16,
                    Children =
                    {
                        new Label
                        {
                            Text = "L'application n'a pas pu démarrer.",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold
                        },
                        new Label
                        {
                            Text = exception.ToString()
                        }
                    }
                }
            }
        };
    }
}
