using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Reviews;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly NestHubDbContext _context;

    public ReviewRepository(NestHubDbContext context) => _context = context;

    public Task<Review?> GetByIdAsync(ReviewId id, CancellationToken cancellationToken = default) =>
        _context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Review>> GetByVendorAndSocietyAsync(VendorId vendorId, SocietyId societyId, CancellationToken cancellationToken = default) =>
        await _context.Reviews
            .Where(r => r.VendorId == vendorId && r.SocietyId == societyId)
            .OrderByDescending(r => r.CreatedDateTimeUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Review>> GetByVendorAsync(VendorId vendorId, CancellationToken cancellationToken = default) =>
        await _context.Reviews.Where(r => r.VendorId == vendorId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Review>> GetFlaggedAsync(CancellationToken cancellationToken = default) =>
        await _context.Reviews.Where(r => r.IsFlagged).OrderByDescending(r => r.CreatedDateTimeUtc).ToListAsync(cancellationToken);

    public void Add(Review review) => _context.Reviews.Add(review);

    public void Remove(Review review) => _context.Reviews.Remove(review);
}
