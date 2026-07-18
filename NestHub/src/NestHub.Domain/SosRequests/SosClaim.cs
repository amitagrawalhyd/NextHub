using NestHub.Domain.Common;

namespace NestHub.Domain.SosRequests;

public sealed class SosClaim : Entity<SosClaimId>
{
    public SosRequestId SosRequestId { get; private set; }
    public VendorId VendorId { get; private set; }
    public DateTime ClaimedDateTimeUtc { get; private set; }

    private SosClaim()
    {
    }

    private SosClaim(SosClaimId id, SosRequestId sosRequestId, VendorId vendorId)
    {
        Id = id;
        SosRequestId = sosRequestId;
        VendorId = vendorId;
        ClaimedDateTimeUtc = DateTime.UtcNow;
    }

    internal static SosClaim Create(SosRequestId sosRequestId, VendorId vendorId) =>
        new(SosClaimId.New(), sosRequestId, vendorId);
}
