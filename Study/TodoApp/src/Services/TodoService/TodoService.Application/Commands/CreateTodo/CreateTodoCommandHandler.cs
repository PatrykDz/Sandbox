using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.Contracts.Events;
using TodoService.Application.DTOs;
using TodoService.Domain.Entities;
using TodoService.Domain.Repositories;

namespace TodoService.Application.Commands.CreateTodo;

public sealed class CreateTodoCommandHandler(
    ITodoRepository todoRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<CreateTodoCommandHandler> logger)
    : ICommandHandler<CreateTodoCommand, TodoDto>
{
    public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = Todo.Create(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssignedToUserId);

        await todoRepository.AddAsync(todo, cancellationToken);

        // MassTransit outbox ensures this message is published reliably
        await publishEndpoint.Publish(new TodoCreatedEvent(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.Priority.ToString(),
            todo.AssignedToUserId,
            todo.DueDate ?? DateTime.UtcNow.AddDays(7),
            todo.CreatedAt), cancellationToken);

        logger.LogInformation("Todo {TodoId} created with title '{Title}'", todo.Id, todo.Title);

        return todo.ToDto();
    }
}
