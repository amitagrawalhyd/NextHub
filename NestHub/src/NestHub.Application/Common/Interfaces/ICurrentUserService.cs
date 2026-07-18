using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Application.Common.Interfaces;

public interface ICurrentUserService
{
    UserId? UserId { get; }
    UserType? UserType { get; }
    bool IsAuthenticated { get; }
}
