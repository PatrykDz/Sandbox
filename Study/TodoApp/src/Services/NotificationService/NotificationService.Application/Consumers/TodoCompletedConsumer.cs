using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public sealed class TodoCompletedConsumer(ILogger<TodoCompletedConsumer> logger)
    : IConsumer<TodoCompletedEvent>
{
    public async Task Consume(ConsumeContext<TodoCompletedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing TodoCompleted notification: TodoId={TodoId}, Title='{Title}', CompletedAt={CompletedAt}",
            message.TodoId,
            message.Title,
            message.CompletedAt);

        if (message.AssignedToUserId.HasValue)
        {
            logger.LogInformation(
                "Sending completion notification to user {UserId} for Todo '{Title}'",
                message.AssignedToUserId,
                message.Title);

            // await notificationSender.SendCompletionAsync(...)
        }

        await Task.CompletedTask;
    }
}
