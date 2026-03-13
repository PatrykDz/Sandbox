using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;
using TodoService.Domain.Exceptions;
using TodoService.Domain.Repositories;

namespace TodoService.Application.Queries.GetTodo;

public sealed class GetTodoQueryHandler(ITodoRepository todoRepository)
    : IQueryHandler<GetTodoQuery, TodoDto>
{
    public async Task<TodoDto> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw TodoDomainException.NotFound(request.Id);

        return todo.ToDto();
    }
}
