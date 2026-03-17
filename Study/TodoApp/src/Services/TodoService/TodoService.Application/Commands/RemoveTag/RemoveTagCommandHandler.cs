using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Commands.RemoveTag;

public sealed class RemoveTagCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveTagCommand, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(RemoveTagCommand request, CancellationToken cancellationToken)
    {
        var id = TodoId.From(request.TodoId);
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);
        if (todo is null) return TodoErrors.NotFound(request.TodoId);

        var result = todo.RemoveTag(request.TagName);
        if (result.IsFailure) return result.Error;

        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
