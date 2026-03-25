using System.Linq.Expressions;
using Shared.BuildingBlocks.Specification;
using TodoService.Domain.Entities;
using TodoService.Domain.Enums;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Domain.Specifications;

public sealed class TodoByStatusSpec(TodoStatus status) : Specification<Todo>
{
    public override Expression<Func<Todo, bool>> ToExpression()
        => todo => todo.Status == status;
}

public sealed class TodoByPrioritySpec(TodoPriority priority) : Specification<Todo>
{
    public override Expression<Func<Todo, bool>> ToExpression()
        => todo => todo.Priority == priority;
}

public sealed class TodoByAssigneeSpec(UserId userId) : Specification<Todo>
{
    public override Expression<Func<Todo, bool>> ToExpression()
        => todo => todo.AssignedToUserId == userId;
}

public sealed class TodoOverdueSpec : Specification<Todo>
{
    public override Expression<Func<Todo, bool>> ToExpression()
        => todo =>
            todo.DueDate.HasValue &&
            todo.DueDate.Value < DateTime.UtcNow &&
            todo.Status != TodoStatus.Completed &&
            todo.Status != TodoStatus.Cancelled;
}

public sealed class TodoSearchSpec(string term) : Specification<Todo>
{
    public override Expression<Func<Todo, bool>> ToExpression()
        => todo =>
            todo.Title.Value.Contains(term) ||
            (todo.Description != null && todo.Description.Value.Contains(term));
}

public sealed class TodoNotDeletedSpec : Specification<Todo>
{
    public override Expression<Func<Todo, bool>> ToExpression()
        => todo => todo.Status != TodoStatus.Cancelled;
}
