using NestHub.Domain.Common;

namespace NestHub.Domain.Residents;

public sealed record ResidentJoinedSocietyDomainEvent(ResidentId ResidentId, SocietyId SocietyId) : DomainEvent;
