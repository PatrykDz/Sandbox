using Shared.BuildingBlocks.CQRS;

namespace TodoService.Application.Commands.DeleteTodo;

public sealed record DeleteTodoCommand(Guid Id) : ICommand;
