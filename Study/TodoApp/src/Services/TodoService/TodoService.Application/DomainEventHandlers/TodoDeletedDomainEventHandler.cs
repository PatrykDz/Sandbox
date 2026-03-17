using MassTransit;
using MediatR;
using Shared.Contracts.Events;
using TodoService.Domain.Events;

namespace TodoService.Application.DomainEventHandlers;

internal sealed class TodoDeletedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<TodoDeletedDomainEvent>
{
    public async Task Handle(TodoDeletedDomainEvent notification, CancellationToken cancellationToken)
        => await publishEndpoint.Publish(
            new TodoDeletedEvent(notification.TodoId, DateTime.UtcNow),
            cancellationToken);
}
