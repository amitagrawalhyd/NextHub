using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestHub.Mobile.Services.Api;

namespace NestHub.Mobile.ViewModels.Resident;

public enum NotificationKind
{
    VendorBroadcast,
    Announcement
}

public sealed record NotificationItem(NotificationKind Kind, string Title, string Body, string SourceName, DateTime CreatedDateTimeUtc);

public sealed partial class NotificationsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public NotificationsViewModel(ApiClient apiClient) => _apiClient = apiClient;

    public ObservableCollection<NotificationItem> Items { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var residentProfile = await _apiClient.GetMyResidentProfileAsync();
            if (residentProfile is null) return;

            var broadcasts = await _apiClient.GetVendorBroadcastFeedAsync(residentProfile.SocietyId);
            var announcements = await _apiClient.GetAnnouncementsAsync(residentProfile.SocietyId);

            var combined = broadcasts
                .Select(b => new NotificationItem(NotificationKind.VendorBroadcast, b.Title, b.Message, b.BusinessName, b.CreatedDateTimeUtc))
                .Concat(announcements.Select(a => new NotificationItem(NotificationKind.Announcement, a.Title, a.Body, "Society Announcement", a.CreatedDateTimeUtc)))
                .OrderByDescending(i => i.CreatedDateTimeUtc)
                .ToList();

            Items.Clear();
            foreach (var item in combined)
                Items.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
