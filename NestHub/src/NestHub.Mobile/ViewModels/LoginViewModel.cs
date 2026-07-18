using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NestHub.Application.Users.Commands.Login;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ILogger<LoginViewModel> _logger;

    public LoginViewModel(ApiClient apiClient, AuthSession authSession, ILogger<LoginViewModel> logger)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _logger = logger;
    }

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private Task NavigateToRegisterAsync() => Shell.Current.GoToAsync("Register");

    [RelayCommand]
    private Task NavigateToHelpAsync() => Shell.Current.GoToAsync("Help");

    [RelayCommand]
    private Task NavigateToPrivacyPolicyAsync() => Shell.Current.GoToAsync("PrivacyPolicy");

    [RelayCommand]
    private Task NavigateToTermsAsync() => Shell.Current.GoToAsync("TermsAndConditions");

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var result = await _apiClient.LoginAsync(new LoginCommand(PhoneNumber, Password));
            await _authSession.SignInAsync(result.Token, result.UserId, result.UserType);

            if (result.UserType == "Vendor")
            {
                var vendorProfile = await _apiClient.GetMyVendorProfileAsync();
                await Shell.Current.GoToAsync(vendorProfile is null ? "VendorOnboarding" : "//VendorShell");
            }
            else
            {
                var residentProfile = await _apiClient.GetMyResidentProfileAsync();
                await Shell.Current.GoToAsync(residentProfile is null ? "ResidentOnboarding" : "//ResidentShell");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Login failed for phone number {PhoneNumber}.", PhoneNumber);
            ErrorMessage = "Invalid phone number or password.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
