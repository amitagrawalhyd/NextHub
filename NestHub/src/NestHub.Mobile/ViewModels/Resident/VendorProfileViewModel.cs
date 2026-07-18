using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Application.Reviews.Commands.SubmitReview;
using NestHub.Application.Reviews.Dtos;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;
using NestHub.Mobile.Services.Native;

namespace NestHub.Mobile.ViewModels.Resident;

[QueryProperty(nameof(VendorId), "vendorId")]
public sealed partial class VendorProfileViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly IPhoneDialerService _phoneDialerService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly INoObligationInterceptor _interceptor;

    private Guid? _residentId;
    private Guid? _societyId;

    public VendorProfileViewModel(
        ApiClient apiClient,
        AuthSession authSession,
        IPhoneDialerService phoneDialerService,
        IWhatsAppService whatsAppService,
        INoObligationInterceptor interceptor)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _phoneDialerService = phoneDialerService;
        _whatsAppService = whatsAppService;
        _interceptor = interceptor;
    }

    [ObservableProperty]
    private string _vendorId = string.Empty;

    [ObservableProperty]
    private VendorDto? _vendor;

    [ObservableProperty]
    private string _newReviewComment = string.Empty;

    [ObservableProperty]
    private int _newReviewRating = 5;

    public ObservableCollection<ReviewDto> NeighborReviews { get; } = new();

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (!Guid.TryParse(VendorId, out var vendorGuid)) return;

        Vendor = await _apiClient.GetVendorProfileAsync(vendorGuid);

        var residentProfile = await _apiClient.GetMyResidentProfileAsync();
        if (residentProfile is null) return;

        _residentId = residentProfile.Id;
        _societyId = residentProfile.SocietyId;

        var reviews = await _apiClient.GetReviewsForVendorInSocietyAsync(vendorGuid, residentProfile.SocietyId);
        NeighborReviews.Clear();
        foreach (var review in reviews)
            NeighborReviews.Add(review);
    }

    [RelayCommand]
    private Task CallAsync() => _interceptor.InterceptAsync(() =>
    {
        if (Vendor is not null) _phoneDialerService.Call(Vendor.WhatsAppNumber);
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task WhatsAppAsync() => _interceptor.InterceptAsync(() =>
        Vendor is null
            ? Task.CompletedTask
            : _whatsAppService.OpenChatAsync(Vendor.WhatsAppNumber, $"Hi, I found your profile on NestHub and I'm interested in your services."));

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (_residentId is null || _societyId is null || !Guid.TryParse(VendorId, out var vendorGuid)) return;

        var review = await _apiClient.SubmitReviewAsync(new SubmitReviewCommand(
            _residentId.Value, vendorGuid, _societyId.Value, NewReviewRating, NewReviewComment));

        NeighborReviews.Insert(0, review);
        NewReviewComment = string.Empty;
    }
}
