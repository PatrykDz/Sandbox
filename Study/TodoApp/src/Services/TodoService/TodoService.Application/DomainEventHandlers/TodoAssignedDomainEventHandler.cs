using MassTransit;
using MediatR;
using Shared.Contracts.Events;
using TodoService.Domain.Events;

namespace TodoService.Application.DomainEventHandlers;

internal sealed class TodoAssignedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<TodoAssignedDomainEvent>
{
    public async Task Handle(TodoAssignedDomainEvent notification, CancellationToken cancellationToken)
        => await publishEndpoint.Publish(
            new TodoAssignedEvent(
                notification.TodoId,
                notification.Title,
                notification.AssignedToUserId,
                notification.PreviousAssigneeId?.Value),
            cancellationToken);
}
