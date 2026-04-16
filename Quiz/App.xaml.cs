using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Quiz.Services;
using Quiz.Views;

namespace Quiz;

public partial class App : Microsoft.Maui.Controls.Application
{
    private Microsoft.Maui.Controls.Page _startupPage;

    public App(IAppInitializer appInitializer, LoginPage loginPage)
    {
        InitializeComponent();

        _startupPage = loginPage;

        try
        {
            appInitializer.InitializeAsync().GetAwaiter().GetResult();
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

    private static Microsoft.Maui.Controls.Page BuildErrorPage(Exception exception)
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
