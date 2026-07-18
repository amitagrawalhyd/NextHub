using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.Repositories;

public interface IVendorFavoriteRepository
{
    Task<VendorFavorite?> GetAsync(ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorFavorite>> GetByResidentIdAsync(ResidentId residentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorFavorite>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(VendorFavorite favorite);
    void Remove(VendorFavorite favorite);
}
