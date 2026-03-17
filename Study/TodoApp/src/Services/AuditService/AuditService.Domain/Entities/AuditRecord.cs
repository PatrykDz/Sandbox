namespace AuditService.Domain.Entities;

/// <summary>
/// Immutable audit record. Once created, never updated or deleted.
/// Represents a projection of all integration events in the system.
/// </summary>
public sealed class AuditRecord
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = default!;
    public string EventVersion { get; private set; } = default!;
    public string AggregateType { get; private set; } = default!;
    public Guid? AggregateId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string Payload { get; private set; } = default!;   // JSON
    public DateTime OccurredAt { get; private set; }
    public string ServiceSource { get; private set; } = default!;

    private AuditRecord() { }

    public static AuditRecord Create(
        string eventType,
        string eventVersion,
        string aggregateType,
        Guid? aggregateId,
        Guid? actorUserId,
        string payloadJson,
        DateTime occurredAt,
        string serviceSource)
    {
        return new AuditRecord
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EventVersion = eventVersion,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            ActorUserId = actorUserId,
            Payload = payloadJson,
            OccurredAt = occurredAt,
            ServiceSource = serviceSource
        };
    }
}
