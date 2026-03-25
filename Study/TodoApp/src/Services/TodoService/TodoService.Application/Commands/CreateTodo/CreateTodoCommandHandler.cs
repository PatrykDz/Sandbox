using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Entities;
using TodoService.Domain.Enums;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;
using TodoService.Domain.ValueObjects;

namespace TodoService.Application.Commands.CreateTodo;

public sealed class CreateTodoCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTodoCommand, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var titleResult = TodoTitle.Create(request.Title);
        if (titleResult.IsFailure) return titleResult.Error;

        TodoDescription? description = null;
        if (request.Description is not null)
        {
            var descResult = TodoDescription.Create(request.Description);
            if (descResult.IsFailure) return descResult.Error;
            description = descResult.Value;
        }

        var userId = request.AssignedToUserId.HasValue
            ? UserId.From(request.AssignedToUserId.Value)
            : (UserId?)null;

        var createResult = Todo.Create(titleResult.Value, description, request.Priority, request.DueDate, userId);
        if (createResult.IsFailure) return createResult.Error;

        var todo = createResult.Value;
        await todoRepository.AddAsync(todo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
