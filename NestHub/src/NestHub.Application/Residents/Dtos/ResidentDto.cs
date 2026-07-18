namespace NestHub.Application.Residents.Dtos;

public sealed record ResidentDto(
    Guid Id,
    Guid UserId,
    Guid SocietyId,
    string BlockNumber,
    string FlatNumber,
    string Name);
