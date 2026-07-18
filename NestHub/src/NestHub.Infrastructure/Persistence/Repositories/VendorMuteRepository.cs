using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class VendorMuteRepository : IVendorMuteRepository
{
    private readonly NestHubDbContext _context;

    public VendorMuteRepository(NestHubDbContext context) => _context = context;

    public Task<VendorMute?> GetAsync(ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default) =>
        _context.VendorMutes.FirstOrDefaultAsync(m => m.ResidentId == residentId && m.VendorId == vendorId, cancellationToken);

    public async Task<IReadOnlyList<VendorMute>> GetByResidentIdAsync(ResidentId residentId, CancellationToken cancellationToken = default) =>
        await _context.VendorMutes.Where(m => m.ResidentId == residentId).ToListAsync(cancellationToken);

    public void Add(VendorMute mute) => _context.VendorMutes.Add(mute);

    public void Remove(VendorMute mute) => _context.VendorMutes.Remove(mute);
}
