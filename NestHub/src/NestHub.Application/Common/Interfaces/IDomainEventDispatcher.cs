using NestHub.Domain.Common;

namespace NestHub.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEventsAsync(IEnumerable<IHasDomainEvents> aggregatesWithEvents, CancellationToken cancellationToken = default);
}
