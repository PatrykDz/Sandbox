using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Commands.CancelTodo;

public sealed record CancelTodoCommand(Guid Id, string Reason) : ICommand<TodoDto>;
