namespace NestHub.Application.Users.Dtos;

public sealed record AuthResultDto(
    Guid UserId,
    string UserType,
    string Token,
    DateTime ExpiresAtUtc);
