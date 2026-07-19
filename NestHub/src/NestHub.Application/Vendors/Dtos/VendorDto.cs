using NestHub.Application.Societies.Dtos;
using NestHub.Application.Vendors.Common;

namespace NestHub.Application.Vendors.Dtos;

public sealed record DailyHoursDto(TimeOnly? OpensAt, TimeOnly? ClosesAt, bool IsClosed);

public sealed record OperatingHoursDto(IReadOnlyDictionary<DayOfWeek, DailyHoursDto> Days);

public sealed record VendorDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string? LogoUrl,
    string? Bio,
    string WhatsAppNumber,
    OperatingHoursDto OperatingHours,
    string SubscriptionTier,
    string TrustBadgeStatus,
    double AverageRating,
    bool IsApproved,
    bool IsFeatured,
    IReadOnlyList<ServiceDto> Services,
    GeoLocationDto? GeoLocation = null,
    string Tier = "Other")
{
    /// <summary>
    /// Computed, not persisted — derived fresh from BusinessName/Bio/Services every time a
    /// VendorDto is built (via ToDto() or a `with` expression), so every existing call site
    /// gets it for free with no changes needed anywhere they construct a VendorDto.
    /// </summary>
    public string PrimaryCategory => VendorCategoryClassifier.Classify(BusinessName, Bio, Services.Select(s => s.Category));
}
