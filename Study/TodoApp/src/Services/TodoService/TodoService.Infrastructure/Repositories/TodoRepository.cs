using Microsoft.EntityFrameworkCore;
using Shared.BuildingBlocks.Specification;
using TodoService.Domain.Entities;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;
using TodoService.Infrastructure.Data;

namespace TodoService.Infrastructure.Repositories;

public sealed class TodoRepository(TodoDbContext dbContext) : ITodoRepository
{
    public async Task<Todo?> GetByIdAsync(TodoId id, CancellationToken cancellationToken = default)
        => await dbContext.Todos
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Todo> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Specification<Todo>? specification = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Todos
            .Include(t => t.Tags)
            .AsNoTracking();

        if (specification is not null)
            query = query.Where(specification.ToExpression());

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }

    public async Task<IReadOnlyList<Todo>> GetAllBySpecificationAsync(
        Specification<Todo> specification,
        CancellationToken cancellationToken = default)
    {
        var items = await dbContext.Todos
            .Include(t => t.Tags)
            .AsNoTracking()
            .Where(specification.ToExpression())
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }

    public async Task AddAsync(Todo todo, CancellationToken cancellationToken = default)
        => await dbContext.Todos.AddAsync(todo, cancellationToken);

    public void Update(Todo todo)
        => dbContext.Todos.Update(todo);

    public void Delete(Todo todo)
        => dbContext.Todos.Remove(todo);

    public async Task<bool> ExistsAsync(TodoId id, CancellationToken cancellationToken = default)
        => await dbContext.Todos.AnyAsync(t => t.Id == id, cancellationToken);
}
