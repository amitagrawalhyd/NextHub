using System.Security.Claims;

namespace NestHub.Admin.Common;

public static class ClaimsPrincipalExtensions
{
    private const string SocietyIdClaimType = "SocietyId";

    /// <summary>Non-null for a society-scoped Admin; null for a Central Admin.</summary>
    public static Guid? GetSocietyId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(SocietyIdClaimType);
        return Guid.TryParse(value, out var societyId) ? societyId : null;
    }

    public static bool IsCentralAdmin(this ClaimsPrincipal user) => user.GetSocietyId() is null;

    public static Claim SocietyIdClaim(Guid societyId) => new(SocietyIdClaimType, societyId.ToString());
}
