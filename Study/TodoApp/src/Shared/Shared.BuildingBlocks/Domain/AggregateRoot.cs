namespace Shared.BuildingBlocks.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Public accessor for raising domain events from outside the aggregate
    /// in rare cases (e.g., delete operations). Use sparingly.
    /// </summary>
    public void RaiseDomainEventPublic(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
