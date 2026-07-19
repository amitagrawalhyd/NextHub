using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class VendorSocietyCoverageRepository : IVendorSocietyCoverageRepository
{
    private readonly NestHubDbContext _context;

    public VendorSocietyCoverageRepository(NestHubDbContext context) => _context = context;

    public async Task<IReadOnlyList<VendorSocietyCoverage>> GetByVendorIdAsync(VendorId vendorId, CancellationToken cancellationToken = default) =>
        await _context.VendorSocietyCoverages.Where(c => c.VendorId == vendorId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VendorSocietyCoverage>> GetAllForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default) =>
        await _context.VendorSocietyCoverages.Where(c => c.SocietyId == societyId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VendorSocietyCoverage>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.VendorSocietyCoverages.ToListAsync(cancellationToken);

    public async Task ReplaceForVendorAsync(VendorId vendorId, IEnumerable<SocietyId> societyIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.VendorSocietyCoverages.Where(c => c.VendorId == vendorId).ToListAsync(cancellationToken);
        _context.VendorSocietyCoverages.RemoveRange(existing);

        foreach (var societyId in societyIds.Distinct())
            _context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(vendorId, societyId, AffiliationType.Manual));
    }

    public Task<VendorSocietyCoverage?> GetInHouseForVendorAsync(VendorId vendorId, CancellationToken cancellationToken = default) =>
        _context.VendorSocietyCoverages
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.AffiliationType == AffiliationType.InHouse, cancellationToken);

    public async Task SetInHouseAsync(VendorId vendorId, SocietyId? societyId, CancellationToken cancellationToken = default)
    {
        // Clear the vendor's current InHouse row (wherever it points), plus — to avoid colliding
        // with the (VendorId, SocietyId) unique index — any stale Manual/Nearby row that already
        // exists for the society about to become InHouse.
        var existing = await _context.VendorSocietyCoverages
            .Where(c => c.VendorId == vendorId && (c.AffiliationType == AffiliationType.InHouse || (societyId != null && c.SocietyId == societyId)))
            .ToListAsync(cancellationToken);
        _context.VendorSocietyCoverages.RemoveRange(existing);

        if (societyId is not null)
            _context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(vendorId, societyId.Value, AffiliationType.InHouse));
    }

    public async Task ReplaceNearbyForVendorAsync(VendorId vendorId, IEnumerable<SocietyId> societyIds, CancellationToken cancellationToken = default)
    {
        var vendorRows = await _context.VendorSocietyCoverages.Where(c => c.VendorId == vendorId).ToListAsync(cancellationToken);

        _context.VendorSocietyCoverages.RemoveRange(vendorRows.Where(c => c.AffiliationType == AffiliationType.Nearby));

        // A society already covered as InHouse/Manual for this vendor can't also get a Nearby
        // row for the same vendor — the (VendorId, SocietyId) unique index forbids two rows for
        // the same pair regardless of AffiliationType.
        var nonNearbySocietyIds = vendorRows
            .Where(c => c.AffiliationType != AffiliationType.Nearby)
            .Select(c => c.SocietyId)
            .ToHashSet();

        foreach (var societyId in societyIds.Distinct().Where(id => !nonNearbySocietyIds.Contains(id)))
            _context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(vendorId, societyId, AffiliationType.Nearby));
    }
}
