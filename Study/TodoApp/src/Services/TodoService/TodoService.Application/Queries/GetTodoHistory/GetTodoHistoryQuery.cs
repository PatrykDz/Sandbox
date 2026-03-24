using MediatR;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;

namespace TodoService.Application.Queries.GetTodoHistory;

/// <summary>Returns the paginated state-change history for a single Todo.</summary>
public sealed record GetTodoHistoryQuery(
    Guid TodoId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<TodoHistoryDto>>>;
