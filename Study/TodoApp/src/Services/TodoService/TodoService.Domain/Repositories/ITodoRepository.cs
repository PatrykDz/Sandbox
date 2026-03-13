using TodoService.Domain.Entities;
using TodoService.Domain.Enums;

namespace TodoService.Domain.Repositories;

public interface ITodoRepository
{
    Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Todo> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        TodoStatus? status = null,
        TodoPriority? priority = null,
        Guid? assignedToUserId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Todo todo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Todo todo, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
