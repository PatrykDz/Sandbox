using MassTransit;
using MediatR;
using Shared.Contracts.Events;
using UserService.Domain.Events;

namespace UserService.Application.DomainEventHandlers;

internal sealed class UserProfileUpdatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<UserProfileUpdatedDomainEvent>
{
    public async Task Handle(UserProfileUpdatedDomainEvent notification, CancellationToken cancellationToken)
        => await publishEndpoint.Publish(
            new UserProfileUpdatedEvent(notification.UserId, notification.Email, notification.FirstName, notification.LastName),
            cancellationToken);
}
