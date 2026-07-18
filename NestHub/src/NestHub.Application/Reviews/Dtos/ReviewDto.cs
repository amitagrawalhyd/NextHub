namespace NestHub.Application.Reviews.Dtos;

public sealed record ReviewDto(
    Guid Id,
    Guid ResidentId,
    Guid VendorId,
    Guid SocietyId,
    int Rating,
    string? Comment,
    double? SentimentScore,
    bool IsFlagged,
    DateTime CreatedDateTimeUtc);
