using NestHub.Domain.Common;

namespace NestHub.Domain.Societies;

public sealed record SocietyRegisteredDomainEvent(SocietyId SocietyId) : DomainEvent;
