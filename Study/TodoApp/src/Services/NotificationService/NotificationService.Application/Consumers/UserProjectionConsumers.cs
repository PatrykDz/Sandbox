using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Data;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

/// <summary>
/// Maintains the local user projection for NotificationService.
/// Anti-corruption layer: we never call UserService directly.
/// Instead we build our own eventual-consistent view of user data
/// by consuming integration events from the UserService bounded context.
/// </summary>
public sealed class UserRegisteredProjectionConsumer(
    NotificationDbContext dbContext,
    ILogger<UserRegisteredProjectionConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;

        var preference = new UserNotificationPreference
        {
            UserId = msg.UserId,
            Email = msg.Email,
            FullName = $"{msg.FirstName} {msg.LastName}",
            EmailNotificationsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.UserNotificationPreferences.Add(preference);
        await dbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("User notification preference created for UserId={UserId}", msg.UserId);
    }
}

public sealed class UserProfileUpdatedProjectionConsumer(
    NotificationDbContext dbContext,
    ILogger<UserProfileUpdatedProjectionConsumer> logger)
    : IConsumer<UserProfileUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserProfileUpdatedEvent> context)
    {
        var msg = context.Message;

        var preference = await dbContext.UserNotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == msg.UserId, context.CancellationToken);

        if (preference is null)
        {
            logger.LogWarning("No preference found for UserId={UserId} during profile update", msg.UserId);
            return;
        }

        preference.Email = msg.Email;
        preference.FullName = $"{msg.FirstName} {msg.LastName}";
        preference.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
