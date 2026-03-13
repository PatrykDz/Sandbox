namespace Shared.Contracts.Events;

public record TodoUpdatedEvent(
    Guid TodoId,
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate,
    DateTime UpdatedAt);
