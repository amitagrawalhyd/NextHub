using System.Net.Http.Headers;
using System.Net.Http.Json;
using NestHub.Application.Analytics.Commands.RecordAnalyticsEvent;
using NestHub.Application.Analytics.Dtos;
using NestHub.Application.Announcements.Dtos;
using NestHub.Application.Categories.Dtos;
using NestHub.Application.EmergencyContacts.Dtos;
using NestHub.Application.Residents.Dtos;
using NestHub.Application.Residents.Commands.RegisterResident;
using NestHub.Application.Reviews.Commands.SubmitReview;
using NestHub.Application.Reviews.Dtos;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.SosRequests.Commands.ClaimSosRequest;
using NestHub.Application.SosRequests.Commands.CreateSosRequest;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Application.Users.Commands.Login;
using NestHub.Application.Users.Commands.RegisterUser;
using NestHub.Application.Users.Dtos;
using NestHub.Application.Vendors.Commands.CreateVendorBroadcast;
using NestHub.Application.Vendors.Commands.RegisterVendor;
using NestHub.Application.Vendors.Dtos;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.Services.Api;

/// <summary>
/// Thin HTTP wrapper over NestHub.API. Reuses Application-layer command/DTO records directly
/// as wire contracts so the mobile client and the API stay structurally in sync without a
/// duplicate set of request/response models.
/// </summary>
public sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSession _authSession;

    public ApiClient(HttpClient httpClient, AuthSession authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    private void ApplyAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = _authSession.Token is null
            ? null
            : new AuthenticationHeaderValue("Bearer", _authSession.Token);
    }

    public async Task<Guid> RegisterAsync(RegisterUserCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task<AuthResultDto> LoginAsync(LoginCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResultDto>())!;
    }

    public async Task<IReadOnlyList<SocietyDto>> GetSocietiesAsync()
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<SocietyDto>>("api/societies")) ?? new();
    }

    public async Task<ResidentDto?> GetMyResidentProfileAsync()
    {
        ApplyAuthHeader();
        var response = await _httpClient.GetAsync("api/residents/me");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResidentDto>();
    }

    public async Task<ResidentDto> RegisterResidentAsync(RegisterResidentCommand command)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/residents", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ResidentDto>())!;
    }

    public async Task<VendorDto?> GetMyVendorProfileAsync()
    {
        ApplyAuthHeader();
        var response = await _httpClient.GetAsync("api/vendors/me");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VendorDto>();
    }

    public async Task<VendorDto> RegisterVendorAsync(RegisterVendorCommand command)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/vendors", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VendorDto>())!;
    }

    public async Task<IReadOnlyList<VendorDto>> SearchVendorsAsync(string? query, string? category, Guid? residentSocietyId = null)
    {
        ApplyAuthHeader();
        var url = $"api/vendors/search?query={Uri.EscapeDataString(query ?? string.Empty)}&category={Uri.EscapeDataString(category ?? string.Empty)}";
        if (residentSocietyId is not null)
            url += $"&residentSocietyId={residentSocietyId}";
        return (await _httpClient.GetFromJsonAsync<List<VendorDto>>(url)) ?? new();
    }

    public async Task<VendorDto> GetVendorProfileAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<VendorDto>($"api/vendors/{vendorId}"))!;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsForVendorInSocietyAsync(Guid vendorId, Guid societyId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<ReviewDto>>($"api/reviews?vendorId={vendorId}&societyId={societyId}")) ?? new();
    }

    public async Task<ReviewDto> SubmitReviewAsync(SubmitReviewCommand command)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/reviews", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReviewDto>())!;
    }

    public async Task<SosRequestDto> CreateSosRequestAsync(CreateSosRequestCommand command)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/sos-requests", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SosRequestDto>())!;
    }

    public async Task<IReadOnlyList<SosRequestDto>> GetOpenSosRequestsAsync(Guid societyId, string category)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<SosRequestDto>>($"api/sos-requests/open?societyId={societyId}&category={Uri.EscapeDataString(category)}")) ?? new();
    }

    public async Task<SosClaimDto> ClaimSosRequestAsync(Guid sosRequestId, Guid vendorId)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsJsonAsync($"api/sos-requests/{sosRequestId}/claim", new ClaimSosRequestCommand(sosRequestId, vendorId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SosClaimDto>())!;
    }

    public async Task RecordAnalyticsEventAsync(RecordAnalyticsEventCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/analytics/events", command);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AnalyticsSummaryDto> GetVendorAnalyticsDashboardAsync(Guid vendorId, DateTime fromUtc, DateTime toUtc)
    {
        ApplyAuthHeader();
        var url = $"api/analytics/vendors/{vendorId}/dashboard?fromUtc={fromUtc:O}&toUtc={toUtc:O}";
        return (await _httpClient.GetFromJsonAsync<AnalyticsSummaryDto>(url))!;
    }

    public async Task<IReadOnlyList<Guid>> GetVendorCoverageAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<Guid>>($"api/vendors/{vendorId}/coverage")) ?? new();
    }

    public async Task SetVendorCoverageAsync(Guid vendorId, IReadOnlyList<Guid> societyIds)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PutAsJsonAsync($"api/vendors/{vendorId}/coverage", new { SocietyIds = societyIds });
        response.EnsureSuccessStatusCode();
    }

    public async Task AddFavoriteAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsync($"api/vendors/{vendorId}/favorite", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveFavoriteAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        var response = await _httpClient.DeleteAsync($"api/vendors/{vendorId}/favorite");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<VendorDto>> GetMyFavoritesAsync()
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<VendorDto>>("api/residents/me/favorites")) ?? new();
    }

    public async Task<IReadOnlyList<AnnouncementDto>> GetAnnouncementsAsync(Guid societyId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<AnnouncementDto>>($"api/announcements?societyId={societyId}")) ?? new();
    }

    public async Task<IReadOnlyList<EmergencyContactDto>> GetEmergencyContactsAsync(Guid societyId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<EmergencyContactDto>>($"api/emergency-contacts?societyId={societyId}")) ?? new();
    }

    public async Task<IReadOnlyList<VendorBroadcastDto>> GetVendorBroadcastFeedAsync(Guid societyId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<VendorBroadcastDto>>($"api/vendor-broadcasts/feed?societyId={societyId}")) ?? new();
    }

    public async Task<IReadOnlyList<VendorBroadcastDto>> GetMyVendorBroadcastsAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<VendorBroadcastDto>>($"api/vendor-broadcasts/mine?vendorId={vendorId}")) ?? new();
    }

    public async Task<VendorBroadcastDto> CreateVendorBroadcastAsync(CreateVendorBroadcastCommand command)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/vendor-broadcasts", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VendorBroadcastDto>())!;
    }

    public async Task DeleteVendorBroadcastAsync(Guid broadcastId)
    {
        ApplyAuthHeader();
        var response = await _httpClient.DeleteAsync($"api/vendor-broadcasts/{broadcastId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/categories")) ?? new();
    }

    public async Task<IReadOnlyList<VendorBroadcastDto>> GetVendorBroadcastsForVendorAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<VendorBroadcastDto>>($"api/vendor-broadcasts/vendor/{vendorId}")) ?? new();
    }

    public async Task MuteVendorAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        var response = await _httpClient.PostAsync($"api/vendors/{vendorId}/mute", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnmuteVendorAsync(Guid vendorId)
    {
        ApplyAuthHeader();
        var response = await _httpClient.DeleteAsync($"api/vendors/{vendorId}/mute");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Guid>> GetMyMutedVendorIdsAsync()
    {
        ApplyAuthHeader();
        return (await _httpClient.GetFromJsonAsync<List<Guid>>("api/residents/me/muted-vendors")) ?? new();
    }
}
