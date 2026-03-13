using Microsoft.EntityFrameworkCore;
using TodoService.Domain.Entities;
using TodoService.Domain.Enums;
using TodoService.Domain.Repositories;
using TodoService.Infrastructure.Data;

namespace TodoService.Infrastructure.Repositories;

public sealed class TodoRepository(TodoDbContext dbContext) : ITodoRepository
{
    public async Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Todos
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Todo> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        TodoStatus? status = null,
        TodoPriority? priority = null,
        Guid? assignedToUserId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Todos
            .Include(t => t.Tags)
            .AsNoTracking();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (assignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(t =>
                EF.Functions.ILike(t.Title, $"%{searchTerm}%") ||
                (t.Description != null && EF.Functions.ILike(t.Description, $"%{searchTerm}%")));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }

    public async Task AddAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        await dbContext.Todos.AddAsync(todo, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        dbContext.Todos.Update(todo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        dbContext.Todos.Remove(todo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Todos.AnyAsync(t => t.Id == id, cancellationToken);
}
