using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Result;
using Shared.BuildingBlocks.Specification;
using TodoService.Application.DTOs;
using TodoService.Domain.Entities;
using TodoService.Domain.Repositories;
using TodoService.Domain.Specifications;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Queries.GetTodos;

public sealed class GetTodosQueryHandler(ITodoRepository todoRepository)
    : IQueryHandler<GetTodosQuery, PagedResult<TodoDto>>
{
    public async Task<Result<PagedResult<TodoDto>>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        Specification<Todo>? spec = null;

        if (request.Status.HasValue)
            spec = spec is null
                ? new TodoByStatusSpec(request.Status.Value)
                : spec.And(new TodoByStatusSpec(request.Status.Value));

        if (request.Priority.HasValue)
            spec = spec is null
                ? new TodoByPrioritySpec(request.Priority.Value)
                : spec.And(new TodoByPrioritySpec(request.Priority.Value));

        if (request.AssignedToUserId.HasValue)
        {
            var assigneeSpec = new TodoByAssigneeSpec(UserId.From(request.AssignedToUserId.Value));
            spec = spec is null ? assigneeSpec : spec.And(assigneeSpec);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchSpec = new TodoSearchSpec(request.SearchTerm);
            spec = spec is null ? searchSpec : spec.And(searchSpec);
        }

        if (request.OverdueOnly)
        {
            var overdueSpec = new TodoOverdueSpec();
            spec = spec is null ? overdueSpec : spec.And(overdueSpec);
        }

        var (items, totalCount) = await todoRepository.GetPagedAsync(
            request.Page, request.PageSize, spec, cancellationToken);

        var dtos = items.Select(t => t.ToDto()).ToList().AsReadOnly();

        return PagedResult<TodoDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
