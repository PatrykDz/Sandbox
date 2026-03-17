namespace Shared.BuildingBlocks.Domain;

/// <summary>
/// Implemented by all aggregate roots. Allows infrastructure (EF Core interceptors)
/// to discover and dispatch domain events without a hard dependency on concrete IDs.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
