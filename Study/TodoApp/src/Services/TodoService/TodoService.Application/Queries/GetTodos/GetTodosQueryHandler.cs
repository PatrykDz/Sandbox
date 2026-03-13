using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using TodoService.Application.DTOs;
using TodoService.Domain.Repositories;

namespace TodoService.Application.Queries.GetTodos;

public sealed class GetTodosQueryHandler(ITodoRepository todoRepository)
    : IQueryHandler<GetTodosQuery, PagedResult<TodoDto>>
{
    public async Task<PagedResult<TodoDto>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await todoRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Status,
            request.Priority,
            request.AssignedToUserId,
            request.SearchTerm,
            cancellationToken);

        var dtos = items.Select(t => t.ToDto()).ToList().AsReadOnly();

        return PagedResult<TodoDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
