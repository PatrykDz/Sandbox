using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;
using TodoService.Domain.Enums;

namespace TodoService.Application.Commands.CreateTodo;

public sealed record CreateTodoCommand(
    string Title,
    string? Description,
    TodoPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId) : ICommand<TodoDto>;
