using NestHub.Domain.Common;
using NestHub.Domain.Societies;

namespace NestHub.Domain.Repositories;

public interface ISocietyRepository
{
    Task<Society?> GetByIdAsync(SocietyId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Society>> GetActiveAsync(CancellationToken cancellationToken = default);
    void Add(Society society);
}
