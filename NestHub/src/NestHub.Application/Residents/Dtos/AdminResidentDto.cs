namespace NestHub.Application.Residents.Dtos;

public sealed record AdminResidentDto(
    Guid Id,
    string Name,
    string PhoneNumber,
    string SocietyName,
    string BlockNumber,
    string FlatNumber,
    bool IsActive,
    Guid SocietyId,
    Guid UserId);
