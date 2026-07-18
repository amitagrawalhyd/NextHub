using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NestHubDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(NestHubDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = _context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        var result = await _context.SaveChangesAsync(cancellationToken);

        await _domainEventDispatcher.DispatchAndClearEventsAsync(aggregatesWithEvents, cancellationToken);

        return result;
    }
}
