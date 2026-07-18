using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Services.Api;

namespace NestHub.Mobile.ViewModels.Resident;

public sealed partial class VendorSearchViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public VendorSearchViewModel(ApiClient apiClient) => _apiClient = apiClient;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<VendorDto> Vendors { get; } = new();

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var results = await _apiClient.SearchVendorsAsync(SearchQuery, null);

            Vendors.Clear();
            foreach (var vendor in results)
                Vendors.Add(vendor);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task AppearingAsync() => SearchAsync();

    [RelayCommand]
    private Task OpenVendorAsync(VendorDto vendor) => Shell.Current.GoToAsync($"VendorProfile?vendorId={vendor.Id}");
}
