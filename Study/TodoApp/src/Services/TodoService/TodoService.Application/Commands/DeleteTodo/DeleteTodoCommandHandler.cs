using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Commands.DeleteTodo;

public sealed class DeleteTodoCommandHandler(
    ITodoRepository todoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTodoCommand>
{
    public async Task<Result> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var id = TodoId.From(request.Id);
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);
        if (todo is null) return TodoErrors.NotFound(request.Id);

        todoRepository.Delete(todo);

        // Raise the deleted event before saving so the interceptor can dispatch it
        todo.RaiseDomainEventPublic(new Domain.Events.TodoDeletedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow, id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
