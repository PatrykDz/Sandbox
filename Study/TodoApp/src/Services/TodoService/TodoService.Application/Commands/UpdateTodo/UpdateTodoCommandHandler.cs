using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;
using TodoService.Domain.ValueObjects;

namespace TodoService.Application.Commands.UpdateTodo;

public sealed class UpdateTodoCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTodoCommand, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var id = TodoId.From(request.Id);
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);
        if (todo is null) return TodoErrors.NotFound(request.Id);

        var titleResult = TodoTitle.Create(request.Title);
        if (titleResult.IsFailure) return titleResult.Error;

        TodoDescription? description = null;
        if (request.Description is not null)
        {
            var descResult = TodoDescription.Create(request.Description);
            if (descResult.IsFailure) return descResult.Error;
            description = descResult.Value;
        }

        var updateResult = todo.Update(titleResult.Value, description, request.Priority, request.DueDate);
        if (updateResult.IsFailure) return updateResult.Error;

        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
