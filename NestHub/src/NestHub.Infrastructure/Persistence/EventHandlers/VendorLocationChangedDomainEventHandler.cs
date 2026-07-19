using MediatR;
using NestHub.Application.Vendors.Commands.RecomputeVendorProximity;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.EventHandlers;

/// <summary>
/// Thin adapter: DomainEventNotification&lt;T&gt; is an Infrastructure-only type (see
/// DomainEventDispatcher/UnitOfWork), so this is where MediatR's domain-event pipeline hands
/// off — but all actual business logic lives in the Application-layer command it delegates to.
/// </summary>
public sealed class VendorLocationChangedDomainEventHandler : INotificationHandler<DomainEventNotification<VendorLocationChangedDomainEvent>>
{
    private readonly ISender _sender;

    public VendorLocationChangedDomainEventHandler(ISender sender) => _sender = sender;

    public Task Handle(DomainEventNotification<VendorLocationChangedDomainEvent> notification, CancellationToken cancellationToken) =>
        _sender.Send(new RecomputeVendorProximityCommand(notification.DomainEvent.VendorId.Value), cancellationToken);
}
