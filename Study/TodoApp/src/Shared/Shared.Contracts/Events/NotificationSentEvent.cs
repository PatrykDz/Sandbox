namespace Shared.Contracts.Events;

public record NotificationSentEvent(
    Guid EventId,
    DateTime OccurredAt,
    string EventVersion,
    Guid NotificationId,
    Guid TodoId,
    Guid RecipientUserId,
    string Channel,
    string NotificationType,
    DateTime SentAt)
{
    public NotificationSentEvent(Guid notificationId, Guid todoId, Guid recipientUserId, string channel, string notificationType)
        : this(Guid.NewGuid(), DateTime.UtcNow, "1.0", notificationId, todoId, recipientUserId, channel, notificationType, DateTime.UtcNow) { }
}
