using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Commands.CompleteTodo;

public sealed record CompleteTodoCommand(Guid Id) : ICommand<TodoDto>;
