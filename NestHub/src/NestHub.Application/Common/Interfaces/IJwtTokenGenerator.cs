using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Application.Common.Interfaces;

public sealed record GeneratedToken(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    GeneratedToken Generate(UserId userId, UserType userType);
}
