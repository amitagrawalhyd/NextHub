namespace NestHub.Application.Societies.Dtos;

public sealed record GeoLocationDto(double Latitude, double Longitude);

public sealed record SocietyDto(
    Guid Id,
    string Name,
    string Address,
    string City,
    GeoLocationDto? GeoLocation,
    bool IsActive,
    DateTime CreatedDateTimeUtc);
