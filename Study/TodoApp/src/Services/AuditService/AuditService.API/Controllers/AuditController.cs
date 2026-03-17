using AuditService.Application.Queries.GetAuditTrail;
using AuditService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AuditService.API.Controllers;

[ApiController]
[Route("api/v1/audit")]
[Produces("application/json")]
public sealed class AuditController(IAuditRepository auditRepository) : ControllerBase
{
    /// <summary>Query the immutable audit trail.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditTrail(
        [FromQuery] GetAuditTrailQuery query,
        CancellationToken cancellationToken)
    {
        var (items, total) = await auditRepository.GetPagedAsync(
            query.Page, query.PageSize, query.AggregateId, query.EventType,
            query.From, query.To, cancellationToken);

        return Ok(new
        {
            items = items.Select(a => new
            {
                a.Id, a.EventType, a.EventVersion, a.AggregateType,
                a.AggregateId, a.ActorUserId, a.OccurredAt, a.ServiceSource
            }),
            totalCount = total,
            page = query.Page,
            pageSize = query.PageSize
        });
    }
}
