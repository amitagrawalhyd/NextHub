using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;
using NestHub.Domain.ValueObjects;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class SocietyRepository : ISocietyRepository
{
    private readonly NestHubDbContext _context;

    public SocietyRepository(NestHubDbContext context) => _context = context;

    public Task<Society?> GetByIdAsync(SocietyId id, CancellationToken cancellationToken = default) =>
        _context.Societies.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Society>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _context.Societies.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(cancellationToken);

    public void Add(Society society) => _context.Societies.Add(society);

    public async Task<IReadOnlyList<Society>> GetWithinRadiusAsync(GeoLocation origin, double radiusKm, CancellationToken cancellationToken = default)
    {
        var originPoint = origin.ToPoint();
        var radiusMeters = radiusKm * 1000.0;

        // Translates to STDistance(...) <= @radiusMeters, hitting the spatial index on Location.
        return await _context.Societies
            .Where(s => s.IsActive && s.Location != null && s.Location.IsWithinDistance(originPoint, radiusMeters))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}
