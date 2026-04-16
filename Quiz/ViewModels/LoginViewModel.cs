using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quiz.Services;

namespace Quiz.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;

    public LoginViewModel(IAuthService authService, IAppNavigator navigator)
    {
        _authService = authService;
        _navigator = navigator;
    }

    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasStatusMessage { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await ExecuteAuthAsync(() => _authService.LoginAsync(Username, Password));
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        await ExecuteAuthAsync(() => _authService.RegisterAsync(Username, Password));
    }

    private async Task ExecuteAuthAsync(Func<Task<(bool Success, string? ErrorMessage)>> action)
    {
        IsBusy = true;
        HasStatusMessage = false;

        try
        {
            var result = await action();
            if (!result.Success)
            {
                StatusMessage = result.ErrorMessage ?? "Une erreur est survenue.";
                HasStatusMessage = true;
                return;
            }

            Password = string.Empty;
            await _navigator.ShowHomeAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            HasStatusMessage = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
