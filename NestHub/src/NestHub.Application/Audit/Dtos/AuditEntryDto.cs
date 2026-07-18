namespace NestHub.Application.Audit.Dtos;

public sealed record AuditEntryDto(DateTime TimestampUtc, string EventType, string Description);
