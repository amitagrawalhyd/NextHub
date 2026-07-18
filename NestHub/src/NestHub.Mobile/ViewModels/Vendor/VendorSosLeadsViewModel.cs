using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;
using NestHub.Mobile.Services.Realtime;

namespace NestHub.Mobile.ViewModels.Vendor;

public sealed partial class VendorSosLeadsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly SosHubClient _sosHubClient;

    private Guid? _vendorId;
    private Guid? _societyId;
    private const string DefaultCategory = "Plumbing";

    public VendorSosLeadsViewModel(ApiClient apiClient, AuthSession authSession, SosHubClient sosHubClient)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _sosHubClient = sosHubClient;
        _sosHubClient.SosRequestCreated += OnSosRequestCreated;
    }

    public ObservableCollection<SosRequestDto> OpenRequests { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var vendor = await _apiClient.GetMyVendorProfileAsync();
            if (vendor is null) return;

            _vendorId = vendor.Id;

            // In a full build-out the vendor would pick their society coverage during onboarding;
            // for now leads are polled/streamed for the vendor's first listed service category.
            var category = vendor.Services.Count > 0 ? vendor.Services[0].Category : DefaultCategory;

            var societies = await _apiClient.GetSocietiesAsync();
            if (societies.Count == 0) return;

            _societyId = societies[0].Id;

            var requests = await _apiClient.GetOpenSosRequestsAsync(_societyId.Value, category);
            OpenRequests.Clear();
            foreach (var request in requests)
                OpenRequests.Add(request);

            await _sosHubClient.ConnectAsync();
            await _sosHubClient.JoinSocietyCategoryGroupAsync(_societyId.Value, category);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClaimAsync(SosRequestDto request)
    {
        if (_vendorId is null) return;

        await _apiClient.ClaimSosRequestAsync(request.Id, _vendorId.Value);
        OpenRequests.Remove(request);
    }

    private void OnSosRequestCreated(SosRequestCreatedMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_societyId != message.SocietyId) return;

            var requests = await _apiClient.GetOpenSosRequestsAsync(message.SocietyId, message.Category);
            var newRequest = requests.FirstOrDefault(r => r.Id == message.SosRequestId);
            if (newRequest is not null && OpenRequests.All(r => r.Id != newRequest.Id))
                OpenRequests.Insert(0, newRequest);
        });
    }
}
