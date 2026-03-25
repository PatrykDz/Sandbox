using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using TodoService.Domain.Events;

namespace TodoService.Application.DomainEventHandlers;

/// <summary>
/// Translates TodoCreatedDomainEvent (internal) into TodoCreatedEvent (integration event).
/// Executed within the same DB transaction via the MassTransit outbox — guaranteed delivery.
/// </summary>
internal sealed class TodoCreatedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<TodoCreatedDomainEventHandler> logger)
    : INotificationHandler<TodoCreatedDomainEvent>
{
    public async Task Handle(TodoCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("Translating domain event {EventId} to integration event", notification.EventId);

        await publishEndpoint.Publish(
            new TodoCreatedEvent(
                notification.TodoId,
                notification.Title,
                notification.Description,
                notification.Priority.ToString(),
                notification.AssignedToUserId,
                notification.DueDate ?? DateTime.UtcNow.AddDays(7),
                DateTime.UtcNow),
            cancellationToken);
    }
}
