using Shared.BuildingBlocks.Domain;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Domain.Entities;

/// <summary>
/// Immutable audit record of every state change within the Todo aggregate.
/// History entries are owned by the Todo aggregate root and always persisted
/// atomically within the same database transaction as the aggregate state.
/// </summary>
public sealed class TodoHistoryEntry : Entity<Guid>
{
    /// <summary>Type of change: Created, Updated, StatusChanged, Assigned, Completed, Cancelled, TagAdded, TagRemoved.</summary>
    public string EventType { get; private set; } = default!;

    /// <summary>Human-readable snapshot of the state before the change (null for Created events).</summary>
    public string? PreviousState { get; private set; }

    /// <summary>Human-readable snapshot of the relevant state after the change.</summary>
    public string CurrentState { get; private set; } = default!;

    /// <summary>Optional reason provided by the user (e.g. cancellation reason).</summary>
    public string? Reason { get; private set; }

    public TodoId TodoId { get; private set; }

    public DateTime OccurredAt { get; private set; }

    // For EF Core
    private TodoHistoryEntry() { }

    internal static TodoHistoryEntry Record(
        TodoId todoId,
        string eventType,
        string? previousState,
        string currentState,
        string? reason = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TodoId = todoId,
            EventType = eventType,
            PreviousState = previousState,
            CurrentState = currentState,
            Reason = reason,
            OccurredAt = DateTime.UtcNow
        };
}
