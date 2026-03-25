namespace Shared.Contracts.Events;

public record TodoAssignedEvent(
    Guid EventId,
    DateTime OccurredAt,
    string EventVersion,
    Guid TodoId,
    string Title,
    Guid AssignedToUserId,
    Guid? PreviousAssigneeId,
    DateTime AssignedAt)
{
    public TodoAssignedEvent(Guid todoId, string title, Guid assignedToUserId, Guid? previousAssigneeId)
        : this(Guid.NewGuid(), DateTime.UtcNow, "1.0", todoId, title, assignedToUserId, previousAssigneeId, DateTime.UtcNow) { }
}
