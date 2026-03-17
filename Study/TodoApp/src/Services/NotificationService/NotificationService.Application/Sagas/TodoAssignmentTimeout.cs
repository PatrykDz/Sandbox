using MassTransit;

namespace NotificationService.Application.Sagas;

/// <summary>Scheduled timeout message for the assignment notification saga.</summary>
public sealed record TodoAssignmentNotificationTimeout
{
    public Guid TodoId { get; init; }
}
