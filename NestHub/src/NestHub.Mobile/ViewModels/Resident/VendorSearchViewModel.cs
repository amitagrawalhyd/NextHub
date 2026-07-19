using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Offline;

namespace NestHub.Mobile.ViewModels.Resident;

public sealed partial class VendorSearchViewModel : ObservableObject
{
    private const string RecentSearchesPreferenceKey = "RecentVendorSearches";
    private const string SearchCacheKey = "vendor_search_all";
    private const int MaxRecentSearches = 5;

    private readonly ApiClient _apiClient;
    private readonly IOfflineCacheService _offlineCache;
    private readonly ILogger<VendorSearchViewModel> _logger;

    private Guid? _residentSocietyId;
    private List<VendorDto> _allResults = new();

    public VendorSearchViewModel(ApiClient apiClient, IOfflineCacheService offlineCache, ILogger<VendorSearchViewModel> logger)
    {
        _apiClient = apiClient;
        _offlineCache = offlineCache;
        _logger = logger;
    }

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _selectedTier = "All";

    [ObservableProperty]
    private bool _isSearchEmpty = true;

    public ObservableCollection<VendorDto> Vendors { get; } = new();
    public ObservableCollection<string> RecentSearches { get; } = new();
    public ObservableCollection<string> TrendingCategories { get; } = new();
    public IReadOnlyList<string> TierChips { get; } = new[] { "All", "InHouse", "Nearby", "Other" };

    partial void OnSearchQueryChanged(string value) => IsSearchEmpty = string.IsNullOrWhiteSpace(value);

    [RelayCommand]
    private async Task AppearingAsync()
    {
        LoadRecentSearches();

        var residentProfile = await _apiClient.GetMyResidentProfileAsync();
        _residentSocietyId = residentProfile?.SocietyId;

        try
        {
            var categories = await _apiClient.GetCategoriesAsync();
            TrendingCategories.Clear();
            foreach (var category in categories.Where(c => c.IsActive).Take(6))
                TrendingCategories.Add(category.Name);
        }
        catch (Exception ex)
        {
            // Trending chips are a convenience, not critical — a failed fetch just leaves them empty.
            _logger.LogWarning(ex, "Failed to fetch trending categories.");
        }

        await SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
                AddRecentSearch(SearchQuery);

            // Client-side tier filtering (below) still expects the full result set, not one
            // page — a large pageSize preserves that behavior on top of the now-paginated API.
            var results = await _apiClient.SearchVendorsAsync(SearchQuery, null, _residentSocietyId, page: 1, pageSize: 200);
            _allResults = results.Items.ToList();
            IsOffline = false;

            if (string.IsNullOrWhiteSpace(SearchQuery))
                await _offlineCache.SetAsync(SearchCacheKey, _allResults);

            ApplyTierFilter();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vendor search failed for query '{Query}'; falling back to the cached result set.", SearchQuery);

            var cached = string.IsNullOrWhiteSpace(SearchQuery) ? await _offlineCache.GetAsync<List<VendorDto>>(SearchCacheKey) : null;
            _allResults = cached ?? new();
            IsOffline = true;
            ApplyTierFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task SearchByChipAsync(string term)
    {
        SearchQuery = term;
        return SearchAsync();
    }

    [RelayCommand]
    private void FilterByTier(string tier)
    {
        SelectedTier = tier;
        ApplyTierFilter();
    }

    private void ApplyTierFilter()
    {
        var filtered = SelectedTier == "All"
            ? _allResults
            : _allResults.Where(v => v.Tier == SelectedTier).ToList();

        Vendors.Clear();
        foreach (var vendor in filtered)
            Vendors.Add(vendor);
    }

    [RelayCommand]
    private Task OpenVendorAsync(VendorDto vendor) => Shell.Current.GoToAsync($"VendorProfile?vendorId={vendor.Id}");

    private void LoadRecentSearches()
    {
        RecentSearches.Clear();
        var json = Preferences.Get(RecentSearchesPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json)) return;

        try
        {
            var saved = JsonSerializer.Deserialize<List<string>>(json) ?? new();
            foreach (var term in saved)
                RecentSearches.Add(term);
        }
        catch (Exception ex)
        {
            // Corrupted preference value — ignore and start fresh.
            _logger.LogWarning(ex, "Failed to parse the stored recent-searches preference; resetting it.");
        }
    }

    private void AddRecentSearch(string term)
    {
        var updated = new List<string> { term };
        updated.AddRange(RecentSearches.Where(t => !t.Equals(term, StringComparison.OrdinalIgnoreCase)));
        updated = updated.Take(MaxRecentSearches).ToList();

        RecentSearches.Clear();
        foreach (var t in updated)
            RecentSearches.Add(t);

        Preferences.Set(RecentSearchesPreferenceKey, JsonSerializer.Serialize(updated));
    }
}
