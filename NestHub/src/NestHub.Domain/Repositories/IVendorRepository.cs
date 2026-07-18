using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.Repositories;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(VendorId id, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByIdWithServicesAsync(VendorId id, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendor>> GetPendingApprovalAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendor>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendor>> GetAllApprovedAsync(CancellationToken cancellationToken = default);
    void Add(Vendor vendor);
}
