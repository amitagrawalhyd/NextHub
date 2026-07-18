using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NestHub.Application.Announcements.Dtos;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Resources.Strings;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Offline;
using NestHub.Mobile.Services.Realtime;

namespace NestHub.Mobile.ViewModels.Resident;

public sealed partial class HomeViewModel : ObservableObject
{
    private const string BroadcastFeedCacheKey = "vendor_broadcast_feed";

    private readonly ApiClient _apiClient;
    private readonly SosHubClient _sosHubClient;
    private readonly IOfflineCacheService _offlineCache;
    private readonly ILogger<HomeViewModel> _logger;
    private Guid? _societyId;

    public HomeViewModel(ApiClient apiClient, SosHubClient sosHubClient, IOfflineCacheService offlineCache, ILogger<HomeViewModel> logger)
    {
        _apiClient = apiClient;
        _sosHubClient = sosHubClient;
        _offlineCache = offlineCache;
        _logger = logger;

        // Safe against duplicate subscription growth: HomeViewModel is registered as a singleton
        // (see MauiProgram.cs), so this constructor — and this subscription — runs exactly once.
        _sosHubClient.VendorBroadcastCreated += OnVendorBroadcastCreated;

        // Re-renders the greeting/section headers instantly when the user switches language,
        // without needing to reload the page.
        LocalizationResourceManager.Instance.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(BlockUnitText));
        };
    }

    public ObservableCollection<AnnouncementDto> Announcements { get; } = new();
    public ObservableCollection<VendorDto> FavoriteVendors { get; } = new();
    public ObservableCollection<VendorBroadcastDto> Broadcasts { get; } = new();
    public IReadOnlyList<SupportedLanguage> SupportedLanguages => LocalizationResourceManager.SupportedLanguages;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Greeting))]
    private string _residentName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BlockUnitText))]
    private string _blockNumber = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BlockUnitText))]
    private string _flatNumber = string.Empty;

    public string Greeting => string.Format(LocalizationResourceManager.Instance["HomeGreeting"], ResidentName);
    public string BlockUnitText => string.Format(LocalizationResourceManager.Instance["HomeBlockUnit"], BlockNumber, FlatNumber);

    [RelayCommand]
    private async Task AppearingAsync()
    {
        var residentProfile = await _apiClient.GetMyResidentProfileAsync();
        if (residentProfile is null) return;

        _societyId = residentProfile.SocietyId;
        ResidentName = residentProfile.Name;
        BlockNumber = residentProfile.BlockNumber;
        FlatNumber = residentProfile.FlatNumber;

        var announcements = await _apiClient.GetAnnouncementsAsync(residentProfile.SocietyId);
        Announcements.Clear();
        foreach (var announcement in announcements)
            Announcements.Add(announcement);

        var favorites = await _apiClient.GetMyFavoritesAsync();
        FavoriteVendors.Clear();
        foreach (var vendor in favorites)
            FavoriteVendors.Add(vendor);

        await LoadBroadcastsAsync(residentProfile.SocietyId);

        await _sosHubClient.ConnectAsync();
        await _sosHubClient.JoinSocietyBroadcastGroupAsync(residentProfile.SocietyId);
    }

    private async Task LoadBroadcastsAsync(Guid societyId)
    {
        try
        {
            var broadcasts = await _apiClient.GetVendorBroadcastFeedAsync(societyId);
            Broadcasts.Clear();
            foreach (var broadcast in broadcasts)
                Broadcasts.Add(broadcast);

            IsOffline = false;
            await _offlineCache.SetAsync(BroadcastFeedCacheKey, broadcasts.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch the vendor broadcast feed for society {SocietyId}; falling back to the cached copy.", societyId);

            var cached = await _offlineCache.GetAsync<List<VendorBroadcastDto>>(BroadcastFeedCacheKey);
            Broadcasts.Clear();
            foreach (var broadcast in cached ?? new())
                Broadcasts.Add(broadcast);

            IsOffline = true;
        }
    }

    private void OnVendorBroadcastCreated(VendorBroadcastCreatedMessage message)
    {
        if (_societyId is null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Broadcasts.Insert(0, new VendorBroadcastDto(message.BroadcastId, message.VendorId, message.BusinessName, message.Title, message.Message, DateTime.UtcNow, null));
        });
    }

    [RelayCommand]
    private Task OpenVendorAsync(VendorDto vendor) => Shell.Current.GoToAsync($"VendorProfile?vendorId={vendor.Id}");

    [RelayCommand]
    private Task OpenNotificationsAsync() => Shell.Current.GoToAsync("Notifications");

    [RelayCommand]
    private void SelectLanguage(string cultureCode) => LocalizationResourceManager.Instance.SetCulture(new CultureInfo(cultureCode));
}
