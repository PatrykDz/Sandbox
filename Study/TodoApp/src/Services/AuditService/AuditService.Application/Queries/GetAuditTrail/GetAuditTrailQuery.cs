namespace AuditService.Application.Queries.GetAuditTrail;

public sealed record GetAuditTrailQuery(
    int Page = 1,
    int PageSize = 50,
    Guid? AggregateId = null,
    string? EventType = null,
    DateTime? From = null,
    DateTime? To = null);
