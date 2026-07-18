using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class VendorFavoriteRepository : IVendorFavoriteRepository
{
    private readonly NestHubDbContext _context;

    public VendorFavoriteRepository(NestHubDbContext context) => _context = context;

    public Task<VendorFavorite?> GetAsync(ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default) =>
        _context.VendorFavorites.FirstOrDefaultAsync(f => f.ResidentId == residentId && f.VendorId == vendorId, cancellationToken);

    public async Task<IReadOnlyList<VendorFavorite>> GetByResidentIdAsync(ResidentId residentId, CancellationToken cancellationToken = default) =>
        await _context.VendorFavorites.Where(f => f.ResidentId == residentId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VendorFavorite>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.VendorFavorites.ToListAsync(cancellationToken);

    public void Add(VendorFavorite favorite) => _context.VendorFavorites.Add(favorite);

    public void Remove(VendorFavorite favorite) => _context.VendorFavorites.Remove(favorite);
}
