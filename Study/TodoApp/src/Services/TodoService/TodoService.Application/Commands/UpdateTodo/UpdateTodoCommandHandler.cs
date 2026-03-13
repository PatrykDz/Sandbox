using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.Contracts.Events;
using TodoService.Application.DTOs;
using TodoService.Domain.Exceptions;
using TodoService.Domain.Repositories;

namespace TodoService.Application.Commands.UpdateTodo;

public sealed class UpdateTodoCommandHandler(
    ITodoRepository todoRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<UpdateTodoCommandHandler> logger)
    : ICommandHandler<UpdateTodoCommand, TodoDto>
{
    public async Task<TodoDto> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw TodoDomainException.NotFound(request.Id);

        todo.Update(request.Title, request.Description, request.Priority, request.DueDate);

        await todoRepository.UpdateAsync(todo, cancellationToken);

        await publishEndpoint.Publish(new TodoUpdatedEvent(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.Priority.ToString(),
            todo.DueDate,
            todo.UpdatedAt!.Value), cancellationToken);

        logger.LogInformation("Todo {TodoId} updated", todo.Id);

        return todo.ToDto();
    }
}
