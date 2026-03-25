using MassTransit;
using MediatR;
using Shared.Contracts.Events;
using UserService.Domain.Events;

namespace UserService.Application.DomainEventHandlers;

internal sealed class UserRegisteredDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
        => await publishEndpoint.Publish(
            new UserRegisteredEvent(notification.UserId, notification.Email, notification.FirstName, notification.LastName),
            cancellationToken);
}
