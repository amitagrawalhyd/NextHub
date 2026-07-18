using NestHub.Domain.Common;

namespace NestHub.Domain.Vendors;

public sealed class VendorSocietyCoverage : Entity<VendorSocietyCoverageId>
{
    public VendorId VendorId { get; private set; }
    public SocietyId SocietyId { get; private set; }

    private VendorSocietyCoverage()
    {
    }

    private VendorSocietyCoverage(VendorSocietyCoverageId id, VendorId vendorId, SocietyId societyId)
    {
        Id = id;
        VendorId = vendorId;
        SocietyId = societyId;
    }

    public static VendorSocietyCoverage Create(VendorId vendorId, SocietyId societyId) =>
        new(VendorSocietyCoverageId.New(), vendorId, societyId);
}
