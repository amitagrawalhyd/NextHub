using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Vendors.Commands.RegisterVendor;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.ViewModels.Vendor;

public sealed partial class SelectableSociety : ObservableObject
{
    public SocietyDto Society { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectableSociety(SocietyDto society) => Society = society;
}

public sealed partial class VendorOnboardingViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;

    public VendorOnboardingViewModel(ApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
    }

    public ObservableCollection<SelectableSociety> Societies { get; } = new();

    [ObservableProperty]
    private string _businessName = string.Empty;

    [ObservableProperty]
    private string _whatsAppNumber = string.Empty;

    [ObservableProperty]
    private string _bio = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (Societies.Count > 0) return;

        var societies = await _apiClient.GetSocietiesAsync();
        foreach (var society in societies)
            Societies.Add(new SelectableSociety(society));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var vendor = await _apiClient.RegisterVendorAsync(new RegisterVendorCommand(_authSession.UserId!.Value, BusinessName, WhatsAppNumber, Bio));

            var coveredSocietyIds = Societies.Where(s => s.IsSelected).Select(s => s.Society.Id).ToList();
            if (coveredSocietyIds.Count > 0)
                await _apiClient.SetVendorCoverageAsync(vendor.Id, coveredSocietyIds);

            await Shell.Current.GoToAsync("//VendorShell");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
