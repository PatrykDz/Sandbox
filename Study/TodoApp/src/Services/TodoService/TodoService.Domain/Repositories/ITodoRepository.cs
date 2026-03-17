using Shared.BuildingBlocks.Specification;
using TodoService.Domain.Entities;
using TodoService.Domain.Enums;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Domain.Repositories;

public interface ITodoRepository
{
    Task<Todo?> GetByIdAsync(TodoId id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Todo> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Specification<Todo>? specification = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Todo>> GetAllBySpecificationAsync(
        Specification<Todo> specification,
        CancellationToken cancellationToken = default);

    Task AddAsync(Todo todo, CancellationToken cancellationToken = default);
    void Update(Todo todo);
    void Delete(Todo todo);
    Task<bool> ExistsAsync(TodoId id, CancellationToken cancellationToken = default);
}
