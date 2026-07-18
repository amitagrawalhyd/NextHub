using NestHub.Domain.Common;

namespace NestHub.Domain.SosRequests;

public sealed record SosRequestCreatedDomainEvent(SosRequestId SosRequestId, SocietyId SocietyId, string Category) : DomainEvent;

public sealed record SosRequestClaimedDomainEvent(SosRequestId SosRequestId, VendorId VendorId) : DomainEvent;

public sealed record SosRequestClosedDomainEvent(SosRequestId SosRequestId) : DomainEvent;
