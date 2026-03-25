using AuditService.Domain.Entities;

namespace AuditService.Domain.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditRecord record, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuditRecord> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? aggregateId = null,
        string? eventType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
