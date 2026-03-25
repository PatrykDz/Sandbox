using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Commands.StartProgress;

public sealed class StartProgressCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<StartProgressCommand, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(StartProgressCommand request, CancellationToken cancellationToken)
    {
        var id = TodoId.From(request.Id);
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);
        if (todo is null) return TodoErrors.NotFound(request.Id);

        var result = todo.StartProgress();
        if (result.IsFailure) return result.Error;

        todoRepository.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
