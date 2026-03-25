namespace Shared.BuildingBlocks.Persistence;

/// <summary>
/// Abstracts a database transaction boundary. Aggregates are persisted
/// and domain events dispatched atomically within a single commit.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
