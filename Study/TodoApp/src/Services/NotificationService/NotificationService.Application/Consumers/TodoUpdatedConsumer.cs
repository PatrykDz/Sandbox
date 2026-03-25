using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public sealed class TodoUpdatedConsumer(ILogger<TodoUpdatedConsumer> logger)
    : IConsumer<TodoUpdatedEvent>
{
    public async Task Consume(ConsumeContext<TodoUpdatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing TodoUpdated event: TodoId={TodoId}, Title='{Title}', UpdatedAt={UpdatedAt}",
            message.TodoId,
            message.Title,
            message.UpdatedAt);

        // Update downstream projections / search index
        await Task.CompletedTask;
    }
}
