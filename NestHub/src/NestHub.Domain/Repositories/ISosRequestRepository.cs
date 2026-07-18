using NestHub.Domain.Common;
using NestHub.Domain.SosRequests;

namespace NestHub.Domain.Repositories;

public interface ISosRequestRepository
{
    Task<SosRequest?> GetByIdAsync(SosRequestId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SosRequest>> GetOpenBySocietyAndCategoryAsync(SocietyId societyId, string category, CancellationToken cancellationToken = default);
    Task<int> CountOpenAsync(CancellationToken cancellationToken = default);
    Task<int> CountOpenBySocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SosRequest>> GetAllInRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    void Add(SosRequest sosRequest);
}
