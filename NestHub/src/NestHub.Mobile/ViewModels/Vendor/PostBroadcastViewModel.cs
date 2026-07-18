using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NestHub.Application.Vendors.Commands.CreateVendorBroadcast;
using NestHub.Mobile.Resources.Strings;
using NestHub.Mobile.Services.Api;

namespace NestHub.Mobile.ViewModels.Vendor;

public sealed partial class PostBroadcastViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<PostBroadcastViewModel> _logger;

    public PostBroadcastViewModel(ApiClient apiClient, ILogger<PostBroadcastViewModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task PostAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Message)) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var vendor = await _apiClient.GetMyVendorProfileAsync();
            if (vendor is null) return;

            await _apiClient.CreateVendorBroadcastAsync(new CreateVendorBroadcastCommand(vendor.Id, Title, Message, null));
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post a vendor broadcast with title '{Title}'.", Title);
            ErrorMessage = LocalizationResourceManager.Instance["PostBroadcastErrorMessage"];
        }
        finally
        {
            IsBusy = false;
        }
    }
}
