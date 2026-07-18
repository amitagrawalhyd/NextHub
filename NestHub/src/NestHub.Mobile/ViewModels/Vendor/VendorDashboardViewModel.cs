using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.ViewModels.Vendor;

public sealed partial class VendorDashboardViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthSession _authSession;

    public VendorDashboardViewModel(ApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
    }

    [ObservableProperty]
    private VendorDto? _vendor;

    [ObservableProperty]
    private Chart? _analyticsChart;

    [ObservableProperty]
    private bool _isBusy;

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
            var summary = await _apiClient.GetVendorAnalyticsDashboardAsync(Vendor.Id, fromUtc, toUtc);

            AnalyticsChart = new BarChart
            {
                Entries = new[]
                {
                    new ChartEntry(summary.ProfileViews) { Label = "Profile Views", ValueLabel = summary.ProfileViews.ToString(), Color = SKColor.Parse("#1F5C99") },
                    new ChartEntry(summary.CallClicks) { Label = "Calls", ValueLabel = summary.CallClicks.ToString(), Color = SKColor.Parse("#F2872E") },
                    new ChartEntry(summary.WhatsAppClicks) { Label = "WhatsApp", ValueLabel = summary.WhatsAppClicks.ToString(), Color = SKColor.Parse("#3E7DBF") }
                },
                LabelTextSize = 28
            };
        }
        finally
        {
            IsBusy = false;
        }
    }
}
