using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Domain;

namespace TodoService.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that dispatches domain events after a successful save.
///
/// Flow:
///   1. Aggregate raises domain events during business operation
///   2. unitOfWork.SaveChangesAsync() writes the aggregate state to PostgreSQL
///   3. This interceptor fires after the write (SavedChangesAsync)
///   4. Domain events are published via MediatR to their handlers
///   5. Handlers call IPublishEndpoint.Publish() → stored in MassTransit outbox table
///   6. Outbox dispatcher forwards messages to RabbitMQ (guaranteed at-least-once)
/// </summary>
public sealed class DomainEventDispatcherInterceptor(
    IMediator mediator,
    ILogger<DomainEventDispatcherInterceptor> logger)
    : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        if (aggregates.Count == 0) return;

        var allEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

        // Clear before dispatching to prevent double-dispatch on nested saves
        foreach (var agg in aggregates)
            agg.ClearDomainEvents();

        logger.LogDebug("Dispatching {EventCount} domain event(s) to MediatR", allEvents.Count);

        foreach (var domainEvent in allEvents)
            await mediator.Publish(domainEvent, cancellationToken);
    }
}
