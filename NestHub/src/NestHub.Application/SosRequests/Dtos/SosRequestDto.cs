namespace NestHub.Application.SosRequests.Dtos;

public sealed record SosClaimDto(
    Guid Id,
    Guid SosRequestId,
    Guid VendorId,
    DateTime ClaimedDateTimeUtc);

public sealed record SosRequestDto(
    Guid Id,
    Guid ResidentId,
    Guid SocietyId,
    string Category,
    string Description,
    string Status,
    DateTime CreatedDateTimeUtc,
    IReadOnlyList<SosClaimDto> Claims);
