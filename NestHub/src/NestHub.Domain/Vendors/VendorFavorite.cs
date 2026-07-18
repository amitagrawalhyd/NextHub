using NestHub.Domain.Common;
using NestHub.Domain.Residents;

namespace NestHub.Domain.Vendors;

public sealed class VendorFavorite : Entity<VendorFavoriteId>
{
    public ResidentId ResidentId { get; private set; }
    public VendorId VendorId { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }

    private VendorFavorite()
    {
    }

    private VendorFavorite(VendorFavoriteId id, ResidentId residentId, VendorId vendorId)
    {
        Id = id;
        ResidentId = residentId;
        VendorId = vendorId;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static VendorFavorite Create(ResidentId residentId, VendorId vendorId) =>
        new(VendorFavoriteId.New(), residentId, vendorId);
}
