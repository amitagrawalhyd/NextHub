using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Domain.Users;

public sealed record UserRegisteredDomainEvent(UserId UserId, UserType UserType) : DomainEvent;

public sealed record UserVerifiedDomainEvent(UserId UserId) : DomainEvent;
