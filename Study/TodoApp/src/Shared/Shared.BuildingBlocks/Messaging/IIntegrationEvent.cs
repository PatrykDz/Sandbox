namespace Shared.BuildingBlocks.Messaging;

/// <summary>
/// Marker interface for integration events published across bounded context boundaries.
/// All integration events MUST be immutable records and versioned.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventVersion { get; }
}
