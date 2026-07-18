using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Analytics;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class AnalyticsLogRepository : IAnalyticsLogRepository
{
    private readonly NestHubDbContext _context;

    public AnalyticsLogRepository(NestHubDbContext context) => _context = context;

    public void Add(AnalyticsLog log) => _context.AnalyticsLogs.Add(log);

    public async Task<IReadOnlyList<AnalyticsLog>> GetByVendorAsync(VendorId vendorId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
        await _context.AnalyticsLogs
            .Where(l => l.VendorId == vendorId && l.CreatedDateTimeUtc >= fromUtc && l.CreatedDateTimeUtc <= toUtc)
            .ToListAsync(cancellationToken);
}
