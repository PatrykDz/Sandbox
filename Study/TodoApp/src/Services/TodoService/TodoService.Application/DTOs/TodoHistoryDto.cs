using TodoService.Domain.Entities;

namespace TodoService.Application.DTOs;

public sealed record TodoHistoryDto(
    Guid Id,
    Guid TodoId,
    string EventType,
    string? PreviousState,
    string CurrentState,
    string? Reason,
    DateTime OccurredAt);

public static class TodoHistoryMappings
{
    public static TodoHistoryDto ToDto(this TodoHistoryEntry entry)
        => new(
            entry.Id,
            entry.TodoId.Value,
            entry.EventType,
            entry.PreviousState,
            entry.CurrentState,
            entry.Reason,
            entry.OccurredAt);
}
