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

    /// <summary>
    /// Plain coordinate entry (no map SDK dependency), same pattern as the Admin portal's
    /// Society/Vendor location edit forms. A future interactive map pin-drop would replace
    /// these two Entry fields without needing to change RegisterVendorCommand's shape.
    /// </summary>
    [ObservableProperty]
    private string _latitude = string.Empty;

    [ObservableProperty]
    private string _longitude = string.Empty;

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
            double? latitude = double.TryParse(Latitude, out var lat) ? lat : null;
            double? longitude = double.TryParse(Longitude, out var lng) ? lng : null;
            var vendor = await _apiClient.RegisterVendorAsync(new RegisterVendorCommand(_authSession.UserId!.Value, BusinessName, WhatsAppNumber, Bio, latitude, longitude));

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
