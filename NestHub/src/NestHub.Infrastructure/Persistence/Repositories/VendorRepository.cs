using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class VendorRepository : IVendorRepository
{
    private readonly NestHubDbContext _context;

    public VendorRepository(NestHubDbContext context) => _context = context;

    public Task<Vendor?> GetByIdAsync(VendorId id, CancellationToken cancellationToken = default) =>
        _context.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<Vendor?> GetByIdWithServicesAsync(VendorId id, CancellationToken cancellationToken = default) =>
        _context.Vendors.Include(v => v.Services).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<Vendor?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) =>
        _context.Vendors.Include(v => v.Services).FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Vendor>> GetPendingApprovalAsync(CancellationToken cancellationToken = default) =>
        await _context.Vendors.Where(v => !v.IsApproved).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vendor>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default) =>
        await _context.Vendors
            .Include(v => v.Services)
            .Where(v => v.IsApproved && v.Services.Any(s => s.Category == category))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vendor>> GetAllApprovedAsync(CancellationToken cancellationToken = default) =>
        await _context.Vendors.Include(v => v.Services).Where(v => v.IsApproved).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vendor>> GetByIdsAsync(IEnumerable<VendorId> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.Vendors.Where(v => idList.Contains(v.Id)).ToListAsync(cancellationToken);
    }

    public void Add(Vendor vendor) => _context.Vendors.Add(vendor);
}
