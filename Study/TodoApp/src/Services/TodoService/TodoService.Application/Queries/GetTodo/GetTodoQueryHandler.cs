using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using TodoService.Application.DTOs;
using TodoService.Domain.Errors;
using TodoService.Domain.Repositories;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Application.Queries.GetTodo;

public sealed class GetTodoQueryHandler(ITodoRepository todoRepository)
    : IQueryHandler<GetTodoQuery, TodoDto>
{
    public async Task<Result<TodoDto>> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        var id = TodoId.From(request.Id);
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);

        if (todo is null) return TodoErrors.NotFound(request.Id);

        return todo.ToDto();
    }
}
