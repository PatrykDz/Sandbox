using Shared.BuildingBlocks.Domain;
using TodoService.Domain.Enums;
using TodoService.Domain.Events;
using TodoService.Domain.Exceptions;

namespace TodoService.Domain.Entities;

public sealed class Todo : AggregateRoot<Guid>
{
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TodoStatus Status { get; private set; }
    public TodoPriority Priority { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<TodoTag> _tags = [];
    public IReadOnlyCollection<TodoTag> Tags => _tags.AsReadOnly();

    private Todo() { }

    public static Todo Create(
        string title,
        string? description,
        TodoPriority priority,
        DateTime? dueDate,
        Guid? assignedToUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Status = TodoStatus.Pending,
            Priority = priority,
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        };

        todo.RaiseDomainEvent(new TodoCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            todo.Id,
            todo.Title,
            todo.Description,
            todo.Priority,
            todo.AssignedToUserId,
            todo.DueDate));

        return todo;
    }

    public void Update(
        string title,
        string? description,
        TodoPriority priority,
        DateTime? dueDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (Status is TodoStatus.Completed or TodoStatus.Cancelled)
            throw TodoDomainException.InvalidTransition(Status.ToString(), "Update");

        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TodoUpdatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Title,
            Description,
            Priority,
            DueDate));
    }

    public void StartProgress()
    {
        if (Status != TodoStatus.Pending)
            throw TodoDomainException.InvalidTransition(Status.ToString(), TodoStatus.InProgress.ToString());

        Status = TodoStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == TodoStatus.Completed)
            throw TodoDomainException.AlreadyCompleted(Id);

        if (Status == TodoStatus.Cancelled)
            throw TodoDomainException.InvalidTransition(Status.ToString(), TodoStatus.Completed.ToString());

        Status = TodoStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TodoCompletedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Title,
            AssignedToUserId));
    }

    public void Cancel(string reason)
    {
        if (Status == TodoStatus.Cancelled)
            throw TodoDomainException.AlreadyCancelled(Id);

        if (Status == TodoStatus.Completed)
            throw TodoDomainException.InvalidTransition(Status.ToString(), TodoStatus.Cancelled.ToString());

        Status = TodoStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TodoCancelledDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            reason));
    }

    public void AssignTo(Guid userId)
    {
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(string name)
    {
        if (_tags.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return;

        _tags.Add(TodoTag.Create(Id, name));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveTag(string name)
    {
        var tag = _tags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (tag is not null)
        {
            _tags.Remove(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
