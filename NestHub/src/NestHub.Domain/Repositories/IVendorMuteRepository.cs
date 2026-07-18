using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.Repositories;

public interface IVendorMuteRepository
{
    Task<VendorMute?> GetAsync(ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VendorMute>> GetByResidentIdAsync(ResidentId residentId, CancellationToken cancellationToken = default);
    void Add(VendorMute mute);
    void Remove(VendorMute mute);
}
