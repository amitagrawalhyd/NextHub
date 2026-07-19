namespace NestHub.Domain.Enums;

/// <summary>
/// Why a VendorSocietyCoverage row exists: Manual is the pre-existing vendor-self-service
/// "societies I cover" list; InHouse and Nearby are new, automation-maintained affiliations
/// derived from the vendor's own geo-location.
/// </summary>
public enum AffiliationType
{
    Manual = 0,
    InHouse = 1,
    Nearby = 2
}
