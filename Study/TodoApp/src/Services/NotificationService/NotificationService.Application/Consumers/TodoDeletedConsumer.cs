using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public sealed class TodoDeletedConsumer(ILogger<TodoDeletedConsumer> logger)
    : IConsumer<TodoDeletedEvent>
{
    public async Task Consume(ConsumeContext<TodoDeletedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing TodoDeleted event: TodoId={TodoId}, DeletedAt={DeletedAt}",
            message.TodoId,
            message.DeletedAt);

        // Cleanup projections, analytics, cached data etc.

        await Task.CompletedTask;
    }
}
