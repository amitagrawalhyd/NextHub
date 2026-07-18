namespace NestHub.Application.Announcements.Dtos;

public sealed record AnnouncementDto(Guid Id, Guid SocietyId, string Title, string Body, DateTime CreatedDateTimeUtc, DateTime? ExpiresAtUtc);

public sealed record AdminAnnouncementDto(Guid Id, string SocietyName, string Title, string Body, DateTime CreatedDateTimeUtc, DateTime? ExpiresAtUtc, bool IsActive);
