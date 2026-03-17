using MassTransit;
using MediatR;
using Shared.Contracts.Events;
using TodoService.Domain.Events;

namespace TodoService.Application.DomainEventHandlers;

internal sealed class TodoCompletedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<TodoCompletedDomainEvent>
{
    public async Task Handle(TodoCompletedDomainEvent notification, CancellationToken cancellationToken)
        => await publishEndpoint.Publish(
            new TodoCompletedEvent(
                notification.TodoId,
                notification.Title,
                notification.AssignedToUserId,
                DateTime.UtcNow),
            cancellationToken);
}
