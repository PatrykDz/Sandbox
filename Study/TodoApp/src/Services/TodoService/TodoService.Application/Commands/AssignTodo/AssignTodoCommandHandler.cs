using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Commands.AssignTodo;

public sealed class AssignTodoCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignTodoCommand, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(AssignTodoCommand request, CancellationToken cancellationToken)
    {
        var todoId = TodoId.From(request.TodoId);
        var todo = await todoRepository.GetByIdAsync(todoId, cancellationToken);
        if (todo is null) return TodoErrors.NotFound(request.TodoId);

        var result = todo.AssignTo(UserId.From(request.UserId));
        if (result.IsFailure) return result.Error;

        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
