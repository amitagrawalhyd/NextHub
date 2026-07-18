using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;
using NestHub.Application.Analytics.Dtos;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Resources.Strings;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.ViewModels.Vendor;

public sealed partial class VendorDashboardViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;
    private AnalyticsSummaryDto? _lastAnalyticsSummary;

    public VendorDashboardViewModel(ApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;

        // Safe against duplicate subscription growth: VendorDashboardViewModel is registered as
        // a singleton (see MauiProgram.cs), so this constructor — and this subscription — runs
        // exactly once. Rebuilds the chart's labels instantly on a language switch.
        LocalizationResourceManager.Instance.PropertyChanged += (_, _) =>
        {
            if (_lastAnalyticsSummary is not null)
                AnalyticsChart = BuildChart(_lastAnalyticsSummary);
        };
    }

    [ObservableProperty]
    private VendorDto? _vendor;

    [ObservableProperty]
    private Chart? _analyticsChart;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<VendorBroadcastDto> MyBroadcasts { get; } = new();
    public IReadOnlyList<SupportedLanguage> SupportedLanguages => LocalizationResourceManager.SupportedLanguages;

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Vendor = await _apiClient.GetMyVendorProfileAsync();
            if (Vendor is null) return;

            var toUtc = DateTime.UtcNow;
            var fromUtc = toUtc.AddDays(-30);
            _lastAnalyticsSummary = await _apiClient.GetVendorAnalyticsDashboardAsync(Vendor.Id, fromUtc, toUtc);
            AnalyticsChart = BuildChart(_lastAnalyticsSummary);

            await LoadBroadcastsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Chart BuildChart(AnalyticsSummaryDto summary)
    {
        var strings = LocalizationResourceManager.Instance;
        return new BarChart
        {
            Entries = new[]
            {
                new ChartEntry(summary.ProfileViews) { Label = strings["ProfileViewsLabel"], ValueLabel = summary.ProfileViews.ToString(), Color = SKColor.Parse("#1F5C99") },
                new ChartEntry(summary.CallClicks) { Label = strings["CallsLabel"], ValueLabel = summary.CallClicks.ToString(), Color = SKColor.Parse("#F2872E") },
                new ChartEntry(summary.WhatsAppClicks) { Label = strings["WhatsAppLabel"], ValueLabel = summary.WhatsAppClicks.ToString(), Color = SKColor.Parse("#3E7DBF") }
            },
            LabelTextSize = 28
        };
    }

    private async Task LoadBroadcastsAsync()
    {
        if (Vendor is null) return;

        var broadcasts = await _apiClient.GetMyVendorBroadcastsAsync(Vendor.Id);
        MyBroadcasts.Clear();
        foreach (var broadcast in broadcasts)
            MyBroadcasts.Add(broadcast);
    }

    [RelayCommand]
    private async Task DeleteBroadcastAsync(VendorBroadcastDto broadcast)
    {
        await _apiClient.DeleteVendorBroadcastAsync(broadcast.Id);
        MyBroadcasts.Remove(broadcast);
    }

    [RelayCommand]
    private Task PostBroadcastAsync() => Shell.Current.GoToAsync("PostBroadcast");

    [RelayCommand]
    private Task OpenHelpAsync() => Shell.Current.GoToAsync("Help");

    [RelayCommand]
    private void SelectLanguage(string cultureCode) => LocalizationResourceManager.Instance.SetCulture(new CultureInfo(cultureCode));
}
