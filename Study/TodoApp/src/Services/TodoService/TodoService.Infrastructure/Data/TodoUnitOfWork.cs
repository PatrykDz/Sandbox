using TodoService.Domain.Repositories;

namespace TodoService.Infrastructure.Data;

/// <summary>
/// Wraps TodoDbContext.SaveChangesAsync to provide a clean abstraction.
/// The DomainEventDispatcherInterceptor fires automatically after each save.
/// </summary>
public sealed class TodoUnitOfWork(TodoDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
