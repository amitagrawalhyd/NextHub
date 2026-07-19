using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Domain.Vendors;

public sealed class VendorSocietyCoverage : Entity<VendorSocietyCoverageId>
{
    public VendorId VendorId { get; private set; }
    public SocietyId SocietyId { get; private set; }
    public AffiliationType AffiliationType { get; private set; }

    private VendorSocietyCoverage()
    {
    }

    private VendorSocietyCoverage(VendorSocietyCoverageId id, VendorId vendorId, SocietyId societyId, AffiliationType affiliationType)
    {
        Id = id;
        VendorId = vendorId;
        SocietyId = societyId;
        AffiliationType = affiliationType;
    }

    public static VendorSocietyCoverage Create(VendorId vendorId, SocietyId societyId, AffiliationType affiliationType) =>
        new(VendorSocietyCoverageId.New(), vendorId, societyId, affiliationType);
}
