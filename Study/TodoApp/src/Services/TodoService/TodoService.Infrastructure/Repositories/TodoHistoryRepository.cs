using Microsoft.EntityFrameworkCore;
using TodoService.Domain.Entities;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;
using TodoService.Infrastructure.Data;

namespace TodoService.Infrastructure.Repositories;

internal sealed class TodoHistoryRepository(TodoDbContext dbContext) : ITodoHistoryRepository
{
    public async Task<(IReadOnlyList<TodoHistoryEntry> Items, int TotalCount)> GetByTodoIdPagedAsync(
        TodoId todoId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.TodoHistoryEntries
            .AsNoTracking()
            .Where(h => h.TodoId == todoId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(h => h.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }
}
