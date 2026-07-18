namespace NestHub.Application.Users.Dtos;

public sealed record AdminUserDto(Guid Id, string PhoneNumber, string UserType, bool IsVerified, bool IsActive);
