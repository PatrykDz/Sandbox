using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;
using TodoService.Domain.Enums;

namespace TodoService.Application.Commands.UpdateTodo;

public sealed record UpdateTodoCommand(
    Guid Id,
    string Title,
    string? Description,
    TodoPriority Priority,
    DateTime? DueDate) : ICommand<TodoDto>;
