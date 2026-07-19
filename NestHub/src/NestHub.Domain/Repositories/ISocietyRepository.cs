using NestHub.Domain.Common;
using NestHub.Domain.Societies;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.Repositories;

public interface ISocietyRepository
{
    Task<Society?> GetByIdAsync(SocietyId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Society>> GetActiveAsync(CancellationToken cancellationToken = default);
    void Add(Society society);

    /// <summary>Active societies whose Location is within radiusKm of origin, using the spatial index.</summary>
    Task<IReadOnlyList<Society>> GetWithinRadiusAsync(GeoLocation origin, double radiusKm, CancellationToken cancellationToken = default);
}
