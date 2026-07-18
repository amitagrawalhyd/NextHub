namespace NestHub.Application.Vendors.Dtos;

public sealed record VendorBroadcastDto(
    Guid Id,
    Guid VendorId,
    string BusinessName,
    string Title,
    string Message,
    DateTime CreatedDateTimeUtc,
    DateTime? ExpiresAtUtc);
