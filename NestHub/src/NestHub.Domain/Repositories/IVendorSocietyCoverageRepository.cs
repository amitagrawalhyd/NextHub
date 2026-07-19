using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.Repositories;

public interface IVendorSocietyCoverageRepository
{
    Task<IReadOnlyList<VendorSocietyCoverage>> GetByVendorIdAsync(VendorId vendorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorSocietyCoverage>> GetAllForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorSocietyCoverage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task ReplaceForVendorAsync(VendorId vendorId, IEnumerable<SocietyId> societyIds, CancellationToken cancellationToken = default);

    Task<VendorSocietyCoverage?> GetInHouseForVendorAsync(VendorId vendorId, CancellationToken cancellationToken = default);

    /// <summary>Upserts/clears the single InHouse row for a vendor (null clears it).</summary>
    Task SetInHouseAsync(VendorId vendorId, SocietyId? societyId, CancellationToken cancellationToken = default);

    /// <summary>Delete-then-recreate only the Nearby-typed rows for a vendor; InHouse/Manual rows are untouched.</summary>
    Task ReplaceNearbyForVendorAsync(VendorId vendorId, IEnumerable<SocietyId> societyIds, CancellationToken cancellationToken = default);
}
