using TodoService.Domain.Entities;

namespace TodoService.Application.DTOs;

public static class TodoMappings
{
    public static TodoDto ToDto(this Todo todo) => new(
        todo.Id,
        todo.Title,
        todo.Description,
        todo.Status.ToString(),
        todo.Priority.ToString(),
        todo.AssignedToUserId,
        todo.DueDate,
        todo.CreatedAt,
        todo.UpdatedAt,
        todo.CompletedAt,
        todo.Tags.Select(t => t.Name).ToList().AsReadOnly());
}
