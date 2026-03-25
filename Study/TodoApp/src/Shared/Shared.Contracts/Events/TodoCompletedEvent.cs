namespace Shared.Contracts.Events;

public record TodoCompletedEvent(
    Guid TodoId,
    string Title,
    Guid? AssignedToUserId,
    DateTime CompletedAt);
