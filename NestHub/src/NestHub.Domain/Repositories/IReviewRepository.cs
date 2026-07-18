using NestHub.Domain.Common;
using NestHub.Domain.Reviews;

namespace NestHub.Domain.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(ReviewId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetByVendorAndSocietyAsync(VendorId vendorId, SocietyId societyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetByVendorAsync(VendorId vendorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetFlaggedAsync(CancellationToken cancellationToken = default);
    void Add(Review review);
    void Remove(Review review);
}
