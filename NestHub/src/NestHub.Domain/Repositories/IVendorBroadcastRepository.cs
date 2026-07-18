using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.Repositories;

public interface IVendorBroadcastRepository
{
    Task<VendorBroadcast?> GetByIdAsync(VendorBroadcastId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorBroadcast>> GetByVendorIdAsync(VendorId vendorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorBroadcast>> GetActiveByVendorIdsAsync(IEnumerable<VendorId> vendorIds, CancellationToken cancellationToken = default);
    void Add(VendorBroadcast broadcast);
    void Remove(VendorBroadcast broadcast);
}
