namespace Shared.Contracts.Events;

public record TodoCreatedEvent(
    Guid TodoId,
    string Title,
    string? Description,
    string Priority,
    Guid? AssignedToUserId,
    DateTime DueDate,
    DateTime CreatedAt);
