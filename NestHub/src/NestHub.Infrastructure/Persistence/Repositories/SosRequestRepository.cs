using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.SosRequests;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class SosRequestRepository : ISosRequestRepository
{
    private readonly NestHubDbContext _context;

    public SosRequestRepository(NestHubDbContext context) => _context = context;

    public Task<SosRequest?> GetByIdAsync(SosRequestId id, CancellationToken cancellationToken = default) =>
        _context.SosRequests.Include(r => r.Claims).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SosRequest>> GetOpenBySocietyAndCategoryAsync(SocietyId societyId, string category, CancellationToken cancellationToken = default) =>
        await _context.SosRequests
            .Include(r => r.Claims)
            .Where(r => r.SocietyId == societyId && r.Category == category && r.Status != SosStatus.Closed)
            .OrderByDescending(r => r.CreatedDateTimeUtc)
            .ToListAsync(cancellationToken);

    public Task<int> CountOpenAsync(CancellationToken cancellationToken = default) =>
        _context.SosRequests.CountAsync(r => r.Status != SosStatus.Closed, cancellationToken);

    public Task<int> CountOpenBySocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default) =>
        _context.SosRequests.CountAsync(r => r.SocietyId == societyId && r.Status != SosStatus.Closed, cancellationToken);

    public async Task<IReadOnlyList<SosRequest>> GetAllInRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
        await _context.SosRequests
            .Where(r => r.CreatedDateTimeUtc >= fromUtc && r.CreatedDateTimeUtc <= toUtc)
            .ToListAsync(cancellationToken);

    public void Add(SosRequest sosRequest) => _context.SosRequests.Add(sosRequest);
}
