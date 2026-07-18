using NestHub.Domain.Common;

namespace NestHub.Domain.Announcements;

public sealed class Announcement : Entity<AnnouncementId>
{
    public SocietyId SocietyId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public DateTime CreatedDateTimeUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsActive => !ExpiresAtUtc.HasValue || ExpiresAtUtc.Value > DateTime.UtcNow;

    private Announcement()
    {
    }

    private Announcement(AnnouncementId id, SocietyId societyId, string title, string body, DateTime? expiresAtUtc)
    {
        Id = id;
        SocietyId = societyId;
        Title = title;
        Body = body;
        CreatedDateTimeUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static Announcement Create(SocietyId societyId, string title, string body, DateTime? expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));

        return new Announcement(AnnouncementId.New(), societyId, title.Trim(), body.Trim(), expiresAtUtc);
    }
}
