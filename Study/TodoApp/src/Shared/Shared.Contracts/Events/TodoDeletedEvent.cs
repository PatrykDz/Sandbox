namespace Shared.Contracts.Events;

public record TodoDeletedEvent(
    Guid TodoId,
    DateTime DeletedAt);
