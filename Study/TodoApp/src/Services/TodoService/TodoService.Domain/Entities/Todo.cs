using Shared.BuildingBlocks.Domain;
using Shared.BuildingBlocks.Result;
using TodoService.Domain.Enums;
using TodoService.Domain.Errors;
using TodoService.Domain.Events;
using TodoService.Domain.StronglyTypedIds;
using TodoService.Domain.ValueObjects;

namespace TodoService.Domain.Entities;

/// <summary>
/// The Todo aggregate root. All invariants are enforced here.
/// State changes produce domain events (dispatched after persistence)
/// and history entries (saved atomically in the same transaction).
/// </summary>
public sealed class Todo : AggregateRoot<TodoId>
{
    public TodoTitle Title { get; private set; } = default!;
    public TodoDescription? Description { get; private set; }
    public TodoStatus Status { get; private set; }
    public TodoPriority Priority { get; private set; }
    public UserId? AssignedToUserId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<TodoTag> _tags = [];
    public IReadOnlyCollection<TodoTag> Tags => _tags.AsReadOnly();

    private readonly List<TodoHistoryEntry> _historyEntries = [];

    /// <summary>
    /// Ordered history of every state change. Persisted atomically with the aggregate.
    /// Append-only — never modified after creation.
    /// </summary>
    public IReadOnlyCollection<TodoHistoryEntry> HistoryEntries => _historyEntries.AsReadOnly();

    // For EF Core
    private Todo() { }

    public static Result<Todo> Create(
        TodoTitle title,
        TodoDescription? description,
        TodoPriority priority,
        DateTime? dueDate,
        UserId? assignedToUserId)
    {
        if (dueDate.HasValue && dueDate.Value <= DateTime.UtcNow)
            return Error.Validation("Todo.DueDatePast", "Due date must be in the future.");

        var todo = new Todo
        {
            Id = TodoId.New(),
            Title = title,
            Description = description,
            Status = TodoStatus.Pending,
            Priority = priority,
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        };

        todo._historyEntries.Add(TodoHistoryEntry.Record(
            todo.Id,
            "Created",
            previousState: null,
            currentState: $"Title={title.Value}; Status=Pending; Priority={priority}" +
                          (dueDate.HasValue ? $"; DueDate={dueDate:O}" : "") +
                          (assignedToUserId.HasValue ? $"; AssignedTo={assignedToUserId.Value.Value}" : "")));

        todo.RaiseDomainEvent(new TodoCreatedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            todo.Id, todo.Title.Value, todo.Description?.Value,
            todo.Priority, todo.AssignedToUserId?.Value, todo.DueDate));

        return todo;
    }

    public Result Update(
        TodoTitle title,
        TodoDescription? description,
        TodoPriority priority,
        DateTime? dueDate)
    {
        if (Status is TodoStatus.Completed or TodoStatus.Cancelled)
            return TodoErrors.CannotUpdateTerminated;

        var previous = $"Title={Title.Value}; Priority={Priority}; DueDate={DueDate:O}; Description={Description?.Value}";
        var current  = $"Title={title.Value}; Priority={priority}; DueDate={dueDate:O}; Description={description?.Value}";

        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "Updated", previous, current));

        RaiseDomainEvent(new TodoUpdatedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            Id, Title.Value, Description?.Value, Priority, DueDate));

        return Result.Success();
    }

    public Result StartProgress()
    {
        if (Status != TodoStatus.Pending)
            return TodoErrors.InvalidTransition(Status.ToString(), TodoStatus.InProgress.ToString());

        var previous = $"Status={Status}";
        Status = TodoStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "StatusChanged", previous, $"Status={Status}"));

        RaiseDomainEvent(new TodoStatusChangedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, TodoStatus.Pending, TodoStatus.InProgress));

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status == TodoStatus.Completed)
            return TodoErrors.AlreadyCompleted;
        if (Status == TodoStatus.Cancelled)
            return TodoErrors.InvalidTransition(Status.ToString(), TodoStatus.Completed.ToString());

        var previous = $"Status={Status}";
        Status = TodoStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "Completed", previous, $"Status=Completed; CompletedAt={CompletedAt:O}"));

        RaiseDomainEvent(new TodoCompletedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            Id, Title.Value, AssignedToUserId?.Value));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == TodoStatus.Cancelled)
            return TodoErrors.AlreadyCancelled;
        if (Status == TodoStatus.Completed)
            return TodoErrors.InvalidTransition(Status.ToString(), TodoStatus.Cancelled.ToString());

        var previous = $"Status={Status}";
        Status = TodoStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "Cancelled", previous, $"Status=Cancelled", reason: reason));

        RaiseDomainEvent(new TodoCancelledDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, reason));

        return Result.Success();
    }

    public Result AssignTo(UserId userId)
    {
        if (AssignedToUserId == userId)
            return TodoErrors.AssignedToSameUser;

        var previousAssignee = AssignedToUserId;
        var previous = previousAssignee.HasValue ? $"AssignedTo={previousAssignee.Value.Value}" : "AssignedTo=<none>";
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "Assigned", previous, $"AssignedTo={userId.Value}"));

        RaiseDomainEvent(new TodoAssignedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            Id, Title.Value, userId, previousAssignee));

        return Result.Success();
    }

    public Result AddTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Todo.TagNameEmpty", "Tag name cannot be empty.");

        name = name.ToLowerInvariant().Trim();

        if (_tags.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return Result.Success(); // Idempotent — not an error

        _tags.Add(TodoTag.Create(Id, name));
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "TagAdded", previousState: null, currentState: $"Tag={name}"));

        return Result.Success();
    }

    public Result RemoveTag(string name)
    {
        var tag = _tags.FirstOrDefault(
            t => t.Name.Equals(name.ToLowerInvariant().Trim(), StringComparison.OrdinalIgnoreCase));

        if (tag is null)
            return Error.NotFound("Todo.TagNotFound", $"Tag '{name}' not found on this todo.");

        _tags.Remove(tag);
        UpdatedAt = DateTime.UtcNow;

        _historyEntries.Add(TodoHistoryEntry.Record(Id, "TagRemoved", previousState: $"Tag={name}", currentState: "Tag removed"));

        return Result.Success();
    }
}
