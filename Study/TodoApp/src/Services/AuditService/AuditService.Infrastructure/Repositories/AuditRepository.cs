using AuditService.Domain.Entities;
using AuditService.Domain.Repositories;
using AuditService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Infrastructure.Repositories;

public sealed class AuditRepository(AuditDbContext dbContext) : IAuditRepository
{
    public async Task AddAsync(AuditRecord record, CancellationToken cancellationToken = default)
        => await dbContext.AuditRecords.AddAsync(record, cancellationToken);

    public async Task<(IReadOnlyList<AuditRecord> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? aggregateId = null, string? eventType = null,
        DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditRecords.AsNoTracking();
        if (aggregateId.HasValue) query = query.Where(a => a.AggregateId == aggregateId);
        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(a => a.EventType == eventType);
        if (from.HasValue) query = query.Where(a => a.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.OccurredAt <= to.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(a => a.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items.AsReadOnly(), total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
