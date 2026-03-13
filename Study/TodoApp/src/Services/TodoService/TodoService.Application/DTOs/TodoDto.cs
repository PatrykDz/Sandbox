namespace TodoService.Application.DTOs;

public sealed record TodoDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CompletedAt,
    IReadOnlyList<string> Tags);
