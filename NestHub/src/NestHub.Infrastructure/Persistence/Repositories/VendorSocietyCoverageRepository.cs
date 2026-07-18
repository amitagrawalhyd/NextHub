using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
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
            _context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(vendorId, societyId));
    }
}
