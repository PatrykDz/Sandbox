using Shared.BuildingBlocks.Domain;

namespace TodoService.Domain.Entities;

public sealed class TodoTag : Entity<Guid>
{
    public Guid TodoId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private TodoTag() { }

    public static TodoTag Create(Guid todoId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new TodoTag
        {
            Id = Guid.NewGuid(),
            TodoId = todoId,
            Name = name.ToLowerInvariant().Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
