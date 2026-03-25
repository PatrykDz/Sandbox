using TodoService.Domain.Entities;

namespace TodoService.Application.DTOs;

public static class TodoMappings
{
    public static TodoDto ToDto(this Todo todo) => new(
        todo.Id.Value,
        todo.Title.Value,
        todo.Description?.Value,
        todo.Status.ToString(),
        todo.Priority.ToString(),
        todo.AssignedToUserId?.Value,
        todo.DueDate,
        todo.CreatedAt,
        todo.UpdatedAt,
        todo.CompletedAt,
        todo.Tags.Select(t => t.Name).OrderBy(n => n).ToList().AsReadOnly());
}
