using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.Vendors.Commands.RegisterVendor;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.ViewModels.Vendor;

public sealed partial class VendorOnboardingViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;

    public VendorOnboardingViewModel(ApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
    }

    [ObservableProperty]
    private string _businessName = string.Empty;

    [ObservableProperty]
    private string _whatsAppNumber = string.Empty;

    [ObservableProperty]
    private string _bio = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _apiClient.RegisterVendorAsync(new RegisterVendorCommand(
                _authSession.UserId!.Value, BusinessName, WhatsAppNumber, Bio));

            await Shell.Current.GoToAsync("//VendorShell");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
