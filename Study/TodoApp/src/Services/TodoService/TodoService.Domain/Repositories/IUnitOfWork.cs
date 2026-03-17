using Shared.BuildingBlocks.Persistence;

namespace TodoService.Domain.Repositories;

/// <summary>
/// TodoService-scoped unit of work. Commits domain changes and dispatches
/// domain events atomically via the EF Core SaveChanges interceptor.
/// </summary>
public interface IUnitOfWork : IUnitOfWork<IUnitOfWork>
{
}

// Generic marker to allow easy injection by type
public interface IUnitOfWork<T> : Shared.BuildingBlocks.Persistence.IUnitOfWork { }
