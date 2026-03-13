using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.Contracts.Events;
using TodoService.Domain.Exceptions;
using TodoService.Domain.Repositories;

namespace TodoService.Application.Commands.DeleteTodo;

public sealed class DeleteTodoCommandHandler(
    ITodoRepository todoRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<DeleteTodoCommandHandler> logger)
    : ICommandHandler<DeleteTodoCommand>
{
    public async Task Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw TodoDomainException.NotFound(request.Id);

        await todoRepository.DeleteAsync(todo, cancellationToken);

        await publishEndpoint.Publish(new TodoDeletedEvent(
            todo.Id,
            DateTime.UtcNow), cancellationToken);

        logger.LogInformation("Todo {TodoId} deleted", todo.Id);
    }
}
