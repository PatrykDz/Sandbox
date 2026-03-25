using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Data;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

/// <summary>
/// Processes a todo assignment: looks up user preferences from the local projection
/// and sends the appropriate notification channel(s).
/// Publishes NotificationSentEvent so the Saga can complete.
/// </summary>
public sealed class TodoAssignedConsumer(
    NotificationDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<TodoAssignedConsumer> logger)
    : IConsumer<TodoAssignedEvent>
{
    public async Task Consume(ConsumeContext<TodoAssignedEvent> context)
    {
        var msg = context.Message;

        var prefs = await dbContext.UserNotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == msg.AssignedToUserId, context.CancellationToken);

        if (prefs is null)
        {
            logger.LogWarning(
                "No notification preferences found for UserId={UserId}, skipping assignment notification for TodoId={TodoId}",
                msg.AssignedToUserId, msg.TodoId);
            return;
        }

        if (!prefs.EmailNotificationsEnabled)
        {
            logger.LogInformation(
                "Email notifications disabled for UserId={UserId}, skipping",
                msg.AssignedToUserId);
            return;
        }

        // In a real system: inject IEmailSender, IPushNotificationSender etc.
        logger.LogInformation(
            "Sending assignment email to {Email} ({FullName}) for Todo '{Title}' (Id={TodoId})",
            prefs.Email, prefs.FullName, msg.Title, msg.TodoId);

        // Simulate sending notification
        // await emailSender.SendAsync(prefs.Email, "You have been assigned a todo", ...);

        // Publish NotificationSentEvent — this completes the saga
        await publishEndpoint.Publish(
            new NotificationSentEvent(
                Guid.NewGuid(),
                msg.TodoId,
                msg.AssignedToUserId,
                "Email",
                "TodoAssigned"),
            context.CancellationToken);
    }
}
