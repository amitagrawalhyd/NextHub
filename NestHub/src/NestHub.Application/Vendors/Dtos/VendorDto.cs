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
    string Tier = "Other");
