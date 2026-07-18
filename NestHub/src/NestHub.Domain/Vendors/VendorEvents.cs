using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Domain.Vendors;

public sealed record VendorRegisteredDomainEvent(VendorId VendorId, UserId UserId) : DomainEvent;

public sealed record VendorApprovedDomainEvent(VendorId VendorId) : DomainEvent;

public sealed record VendorUpgradedToPremiumDomainEvent(VendorId VendorId) : DomainEvent;

public sealed record TrustBadgeAwardedDomainEvent(VendorId VendorId, TrustBadgeStatus Badge) : DomainEvent;

public sealed record ServiceAddedDomainEvent(VendorId VendorId, ServiceId ServiceId) : DomainEvent;
