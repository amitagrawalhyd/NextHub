namespace NestHub.Application.Admin.SocietyAdmins.Dtos;

public sealed record SocietyAdminDto(Guid Id, string PhoneNumber, string? Email, Guid SocietyId, string SocietyName, bool IsActive, bool IsVerified);
