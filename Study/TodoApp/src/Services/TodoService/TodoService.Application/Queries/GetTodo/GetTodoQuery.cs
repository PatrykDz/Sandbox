using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Queries.GetTodo;

public sealed record GetTodoQuery(Guid Id) : IQuery<TodoDto>;
