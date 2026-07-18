namespace NestHub.Application.Analytics.Dtos;

public sealed record AnalyticsSummaryDto(
    Guid VendorId,
    int ProfileViews,
    int CallClicks,
    int WhatsAppClicks,
    DateTime FromUtc,
    DateTime ToUtc);

public sealed record TopVendorDto(
    Guid VendorId,
    string BusinessName,
    int ProfileViews,
    int CallClicks,
    int WhatsAppClicks,
    int TotalEngagements);
