using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Announcements;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class AnnouncementRepository : IAnnouncementRepository
{
    private readonly NestHubDbContext _context;

    public AnnouncementRepository(NestHubDbContext context) => _context = context;

    public Task<Announcement?> GetByIdAsync(AnnouncementId id, CancellationToken cancellationToken = default) =>
        _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Announcement>> GetActiveForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default) =>
        await _context.Announcements
            .Where(a => a.SocietyId == societyId && (a.ExpiresAtUtc == null || a.ExpiresAtUtc > DateTime.UtcNow))
            .OrderByDescending(a => a.CreatedDateTimeUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Announcement>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Announcements.OrderByDescending(a => a.CreatedDateTimeUtc).ToListAsync(cancellationToken);

    public void Add(Announcement announcement) => _context.Announcements.Add(announcement);

    public void Remove(Announcement announcement) => _context.Announcements.Remove(announcement);
}
