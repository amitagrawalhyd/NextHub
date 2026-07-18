using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class VendorBroadcastRepository : IVendorBroadcastRepository
{
    private readonly NestHubDbContext _context;

    public VendorBroadcastRepository(NestHubDbContext context) => _context = context;

    public Task<VendorBroadcast?> GetByIdAsync(VendorBroadcastId id, CancellationToken cancellationToken = default) =>
        _context.VendorBroadcasts.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<VendorBroadcast>> GetByVendorIdAsync(VendorId vendorId, CancellationToken cancellationToken = default) =>
        await _context.VendorBroadcasts
            .Where(b => b.VendorId == vendorId)
            .OrderByDescending(b => b.CreatedDateTimeUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VendorBroadcast>> GetActiveByVendorIdsAsync(IEnumerable<VendorId> vendorIds, CancellationToken cancellationToken = default) =>
        await _context.VendorBroadcasts
            .Where(b => vendorIds.Contains(b.VendorId) && (b.ExpiresAtUtc == null || b.ExpiresAtUtc > DateTime.UtcNow))
            .OrderByDescending(b => b.CreatedDateTimeUtc)
            .ToListAsync(cancellationToken);

    public void Add(VendorBroadcast broadcast) => _context.VendorBroadcasts.Add(broadcast);

    public void Remove(VendorBroadcast broadcast) => _context.VendorBroadcasts.Remove(broadcast);
}
