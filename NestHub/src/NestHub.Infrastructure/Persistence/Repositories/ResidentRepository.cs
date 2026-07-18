using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Residents;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class ResidentRepository : IResidentRepository
{
    private readonly NestHubDbContext _context;

    public ResidentRepository(NestHubDbContext context) => _context = context;

    public Task<Resident?> GetByIdAsync(ResidentId id, CancellationToken cancellationToken = default) =>
        _context.Residents.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Resident?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) =>
        _context.Residents.FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Resident>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Residents.ToListAsync(cancellationToken);

    public void Add(Resident resident) => _context.Residents.Add(resident);
}
