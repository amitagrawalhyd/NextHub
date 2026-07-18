namespace NestHub.Application.Vendors.Dtos;

public sealed record ServiceDto(
    Guid Id,
    Guid VendorId,
    string Title,
    string Description,
    string Pricing,
    string Category);
