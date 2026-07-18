using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public UserId? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var guid) ? new UserId(guid) : null;
        }
    }

    public UserType? UserType
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserType>(value, out var userType) ? userType : null;
        }
    }
}
