using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.SosRequests.Commands.CreateSosRequest;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Realtime;

namespace NestHub.Mobile.ViewModels.Resident;

public sealed partial class SosViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly SosHubClient _sosHubClient;

    public SosViewModel(ApiClient apiClient, SosHubClient sosHubClient)
    {
        _apiClient = apiClient;
        _sosHubClient = sosHubClient;
        _sosHubClient.SosRequestClaimed += OnSosRequestClaimed;
    }

    [ObservableProperty]
    private string _category = "Plumbing";

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _confirmationMessage;

    private Guid? _residentId;
    private Guid? _societyId;

    [RelayCommand]
    private async Task AppearingAsync()
    {
        var residentProfile = await _apiClient.GetMyResidentProfileAsync();
        if (residentProfile is null) return;

        _residentId = residentProfile.Id;
        _societyId = residentProfile.SocietyId;

        await _sosHubClient.ConnectAsync();
        await _sosHubClient.JoinResidentGroupAsync(_residentId.Value);
    }

    [RelayCommand]
    private async Task SendSosAsync()
    {
        if (_residentId is null || _societyId is null || IsBusy) return;

        try
        {
            IsBusy = true;
            ConfirmationMessage = null;

            await _apiClient.CreateSosRequestAsync(new CreateSosRequestCommand(
                _residentId.Value, _societyId.Value, Category, Description));

            ConfirmationMessage = "Your urgent request has been broadcast to nearby vendors.";
            Description = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnSosRequestClaimed(SosRequestClaimedMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConfirmationMessage = "A vendor has claimed your request and will be in touch shortly.";
        });
    }
}
