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
    private List<Guid> _coveredSocietyIds = new();
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

            var category = vendor.Services.Count > 0 ? vendor.Services[0].Category : DefaultCategory;

            _coveredSocietyIds = (await _apiClient.GetVendorCoverageAsync(vendor.Id)).ToList();
            if (_coveredSocietyIds.Count == 0) return;

            OpenRequests.Clear();
            await _sosHubClient.ConnectAsync();

            foreach (var societyId in _coveredSocietyIds)
            {
                var requests = await _apiClient.GetOpenSosRequestsAsync(societyId, category);
                foreach (var request in requests)
                    OpenRequests.Add(request);

                await _sosHubClient.JoinSocietyCategoryGroupAsync(societyId, category);
            }
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
            if (!_coveredSocietyIds.Contains(message.SocietyId)) return;

            var requests = await _apiClient.GetOpenSosRequestsAsync(message.SocietyId, message.Category);
            var newRequest = requests.FirstOrDefault(r => r.Id == message.SosRequestId);
            if (newRequest is not null && OpenRequests.All(r => r.Id != newRequest.Id))
                OpenRequests.Insert(0, newRequest);
        });
    }
}
