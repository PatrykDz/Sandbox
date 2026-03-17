using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Commands.AssignTodo;

public sealed record AssignTodoCommand(Guid TodoId, Guid UserId) : ICommand<TodoDto>;
