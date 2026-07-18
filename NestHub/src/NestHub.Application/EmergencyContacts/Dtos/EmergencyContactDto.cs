namespace NestHub.Application.EmergencyContacts.Dtos;

public sealed record EmergencyContactDto(Guid Id, Guid SocietyId, string Name, string Role, string PhoneNumber);

public sealed record AdminEmergencyContactDto(Guid Id, string SocietyName, string Name, string Role, string PhoneNumber);
