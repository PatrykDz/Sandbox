using MassTransit;
using MediatR;
using Shared.Contracts.Events;
using TodoService.Domain.Events;

namespace TodoService.Application.DomainEventHandlers;

internal sealed class TodoUpdatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<TodoUpdatedDomainEvent>
{
    public async Task Handle(TodoUpdatedDomainEvent notification, CancellationToken cancellationToken)
        => await publishEndpoint.Publish(
            new TodoUpdatedEvent(
                notification.TodoId,
                notification.Title,
                notification.Description,
                notification.Priority.ToString(),
                notification.DueDate,
                DateTime.UtcNow),
            cancellationToken);
}
