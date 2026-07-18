using NestHub.Domain.Analytics;
using NestHub.Domain.Common;

namespace NestHub.Domain.Repositories;

public interface IAnalyticsLogRepository
{
    void Add(AnalyticsLog log);
    Task<IReadOnlyList<AnalyticsLog>> GetByVendorAsync(VendorId vendorId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnalyticsLog>> GetAllInRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
