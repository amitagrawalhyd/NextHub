using NestHub.Domain.Common;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.Reviews;

public sealed record ReviewSubmittedDomainEvent(ReviewId ReviewId, VendorId VendorId, SocietyId SocietyId, Rating Rating) : DomainEvent;

public sealed record ReviewFlaggedDomainEvent(ReviewId ReviewId) : DomainEvent;

public sealed record ReviewRemovedDomainEvent(ReviewId ReviewId, VendorId VendorId) : DomainEvent;
