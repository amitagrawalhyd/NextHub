using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.Repositories;

public interface IVendorSocietyCoverageRepository
{
    Task<IReadOnlyList<VendorSocietyCoverage>> GetByVendorIdAsync(VendorId vendorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorSocietyCoverage>> GetAllForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorSocietyCoverage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task ReplaceForVendorAsync(VendorId vendorId, IEnumerable<SocietyId> societyIds, CancellationToken cancellationToken = default);
}
