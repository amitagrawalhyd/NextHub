using NestHub.Domain.Announcements;
using NestHub.Domain.Common;

namespace NestHub.Domain.Repositories;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(AnnouncementId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Announcement>> GetActiveForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Announcement>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Announcement announcement);
    void Remove(Announcement announcement);
}
