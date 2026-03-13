using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using TodoService.Application.DTOs;
using TodoService.Domain.Enums;

namespace TodoService.Application.Queries.GetTodos;

public sealed record GetTodosQuery(
    int Page = 1,
    int PageSize = 20,
    TodoStatus? Status = null,
    TodoPriority? Priority = null,
    Guid? AssignedToUserId = null,
    string? SearchTerm = null) : IQuery<PagedResult<TodoDto>>;
