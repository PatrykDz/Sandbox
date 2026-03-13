using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.Contracts.Events;
using TodoService.Application.DTOs;
using TodoService.Domain.Exceptions;
using TodoService.Domain.Repositories;

namespace TodoService.Application.Commands.CompleteTodo;

public sealed class CompleteTodoCommandHandler(
    ITodoRepository todoRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<CompleteTodoCommandHandler> logger)
    : ICommandHandler<CompleteTodoCommand, TodoDto>
{
    public async Task<TodoDto> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw TodoDomainException.NotFound(request.Id);

        todo.Complete();

        await todoRepository.UpdateAsync(todo, cancellationToken);

        await publishEndpoint.Publish(new TodoCompletedEvent(
            todo.Id,
            todo.Title,
            todo.AssignedToUserId,
            todo.CompletedAt!.Value), cancellationToken);

        logger.LogInformation("Todo {TodoId} completed", todo.Id);

        return todo.ToDto();
    }
}
