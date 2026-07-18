using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.Users.Commands.RegisterUser;
using NestHub.Domain.Enums;
using NestHub.Mobile.Services.Api;

namespace NestHub.Mobile.ViewModels;

public sealed partial class RegisterViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public RegisterViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isVendor;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _apiClient.RegisterAsync(new RegisterUserCommand(
                PhoneNumber, null, Password, IsVendor ? UserType.Vendor : UserType.Resident));

            await Shell.Current.GoToAsync("//Login");
        }
        catch (Exception)
        {
            ErrorMessage = "Registration failed. The phone number may already be in use.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
