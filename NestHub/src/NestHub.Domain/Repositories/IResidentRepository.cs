using NestHub.Domain.Common;
using NestHub.Domain.Residents;

namespace NestHub.Domain.Repositories;

public interface IResidentRepository
{
    Task<Resident?> GetByIdAsync(ResidentId id, CancellationToken cancellationToken = default);
    Task<Resident?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Resident>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Resident resident);
}
