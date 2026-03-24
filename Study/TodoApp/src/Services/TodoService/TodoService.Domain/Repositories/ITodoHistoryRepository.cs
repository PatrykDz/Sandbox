using TodoService.Domain.Entities;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Domain.Repositories;

public interface ITodoHistoryRepository
{
    Task<(IReadOnlyList<TodoHistoryEntry> Items, int TotalCount)> GetByTodoIdPagedAsync(
        TodoId todoId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
