using System.Text.Json;
using AuditService.Domain.Entities;
using AuditService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace AuditService.Application.Consumers;

/// <summary>
/// Consumes all Todo-related integration events and appends them to the audit log.
/// Uses idempotency via the MassTransit inbox pattern.
/// </summary>
public sealed class TodoCreatedAuditConsumer(IAuditRepository auditRepository, ILogger<TodoCreatedAuditConsumer> logger)
    : IConsumer<TodoCreatedEvent>
{
    public async Task Consume(ConsumeContext<TodoCreatedEvent> context)
    {
        var msg = context.Message;
        logger.LogDebug("Auditing TodoCreated for TodoId={TodoId}", msg.TodoId);
        var record = AuditRecord.Create(
            nameof(TodoCreatedEvent), "1.0", "Todo", msg.TodoId,
            null, JsonSerializer.Serialize(msg), msg.CreatedAt, "TodoService");
        await auditRepository.AddAsync(record, context.CancellationToken);
        await auditRepository.SaveChangesAsync(context.CancellationToken);
    }
}

public sealed class TodoUpdatedAuditConsumer(IAuditRepository auditRepository)
    : IConsumer<TodoUpdatedEvent>
{
    public async Task Consume(ConsumeContext<TodoUpdatedEvent> context)
    {
        var msg = context.Message;
        var record = AuditRecord.Create(
            nameof(TodoUpdatedEvent), "1.0", "Todo", msg.TodoId,
            null, JsonSerializer.Serialize(msg), msg.UpdatedAt, "TodoService");
        await auditRepository.AddAsync(record, context.CancellationToken);
        await auditRepository.SaveChangesAsync(context.CancellationToken);
    }
}

public sealed class TodoCompletedAuditConsumer(IAuditRepository auditRepository)
    : IConsumer<TodoCompletedEvent>
{
    public async Task Consume(ConsumeContext<TodoCompletedEvent> context)
    {
        var msg = context.Message;
        var record = AuditRecord.Create(
            nameof(TodoCompletedEvent), "1.0", "Todo", msg.TodoId,
            msg.AssignedToUserId, JsonSerializer.Serialize(msg), msg.CompletedAt, "TodoService");
        await auditRepository.AddAsync(record, context.CancellationToken);
        await auditRepository.SaveChangesAsync(context.CancellationToken);
    }
}

public sealed class TodoDeletedAuditConsumer(IAuditRepository auditRepository)
    : IConsumer<TodoDeletedEvent>
{
    public async Task Consume(ConsumeContext<TodoDeletedEvent> context)
    {
        var msg = context.Message;
        var record = AuditRecord.Create(
            nameof(TodoDeletedEvent), "1.0", "Todo", msg.TodoId,
            null, JsonSerializer.Serialize(msg), msg.DeletedAt, "TodoService");
        await auditRepository.AddAsync(record, context.CancellationToken);
        await auditRepository.SaveChangesAsync(context.CancellationToken);
    }
}

public sealed class UserRegisteredAuditConsumer(IAuditRepository auditRepository)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;
        var record = AuditRecord.Create(
            nameof(UserRegisteredEvent), "1.0", "User", msg.UserId,
            msg.UserId, JsonSerializer.Serialize(msg), msg.RegisteredAt, "UserService");
        await auditRepository.AddAsync(record, context.CancellationToken);
        await auditRepository.SaveChangesAsync(context.CancellationToken);
    }
}
