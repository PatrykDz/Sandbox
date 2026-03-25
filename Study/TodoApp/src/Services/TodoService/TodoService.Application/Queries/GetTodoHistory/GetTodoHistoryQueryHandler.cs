using MediatR;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Queries.GetTodoHistory;

internal sealed class GetTodoHistoryQueryHandler(
    ITodoRepository todoRepository,
    ITodoHistoryRepository historyRepository,
    ILogger<GetTodoHistoryQueryHandler> logger)
    : IRequestHandler<GetTodoHistoryQuery, Result<PagedResult<TodoHistoryDto>>>
{
    public async Task<Result<PagedResult<TodoHistoryDto>>> Handle(
        GetTodoHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var todoId = TodoId.From(request.TodoId);

        var exists = await todoRepository.ExistsAsync(todoId, cancellationToken);
        if (!exists)
            return Error.NotFound("Todo.NotFound", $"Todo {request.TodoId} was not found.");

        logger.LogDebug("Querying history for todo {TodoId}, page {Page}", request.TodoId, request.Page);

        var (items, total) = await historyRepository.GetByTodoIdPagedAsync(
            todoId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(e => e.ToDto()).ToList();
        return PagedResult<TodoHistoryDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
