using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Common;

namespace NestHub.Infrastructure.Persistence;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher) => _publisher = publisher;

    public async Task DispatchAndClearEventsAsync(IEnumerable<IHasDomainEvents> aggregatesWithEvents, CancellationToken cancellationToken = default)
    {
        var aggregates = aggregatesWithEvents.ToList();
        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, cancellationToken);
        }
    }
}
