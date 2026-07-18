using NestHub.Domain.Common;
using NestHub.Domain.Residents;

namespace NestHub.Domain.Vendors;

/// <summary>A resident opting out of a specific vendor's broadcasts (announcements) feed.</summary>
public sealed class VendorMute : Entity<VendorMuteId>
{
    public ResidentId ResidentId { get; private set; }
    public VendorId VendorId { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }

    private VendorMute()
    {
    }

    private VendorMute(VendorMuteId id, ResidentId residentId, VendorId vendorId)
    {
        Id = id;
        ResidentId = residentId;
        VendorId = vendorId;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static VendorMute Create(ResidentId residentId, VendorId vendorId) =>
        new(VendorMuteId.New(), residentId, vendorId);
}
