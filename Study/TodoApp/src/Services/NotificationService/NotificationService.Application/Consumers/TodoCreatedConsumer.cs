using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public sealed class TodoCreatedConsumer(ILogger<TodoCreatedConsumer> logger)
    : IConsumer<TodoCreatedEvent>
{
    public async Task Consume(ConsumeContext<TodoCreatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing TodoCreated notification: TodoId={TodoId}, Title='{Title}', Priority={Priority}",
            message.TodoId,
            message.Title,
            message.Priority);

        // In a real system, this would:
        // - Send email/push notification to assigned user
        // - Update a read model / projection
        // - Trigger a webhook
        // - Update analytics

        if (message.AssignedToUserId.HasValue)
        {
            logger.LogInformation(
                "Sending assignment notification to user {UserId} for Todo {TodoId}",
                message.AssignedToUserId,
                message.TodoId);

            // await notificationSender.SendAsync(new AssignmentNotification(...));
        }

        await Task.CompletedTask;
    }
}
