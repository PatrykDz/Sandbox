using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Commands.AddTag;

public sealed class AddTagCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddTagCommand, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(AddTagCommand request, CancellationToken cancellationToken)
    {
        var id = TodoId.From(request.TodoId);
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);
        if (todo is null) return TodoErrors.NotFound(request.TodoId);

        var result = todo.AddTag(request.TagName);
        if (result.IsFailure) return result.Error;

        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
