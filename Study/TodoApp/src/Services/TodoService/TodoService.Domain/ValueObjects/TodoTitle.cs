using Shared.BuildingBlocks.Domain;
using Shared.BuildingBlocks.Result;
using TodoService.Domain.Errors;

namespace TodoService.Domain.ValueObjects;

public sealed class TodoTitle : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private TodoTitle(string value) => Value = value;

    public static Result<TodoTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return TodoErrors.TitleRequired;

        value = value.Trim();
        if (value.Length > MaxLength)
            return TodoErrors.TitleTooLong;

        return new TodoTitle(value);
    }

    /// <summary>Used by EF Core to reconstitute from trusted DB data — bypasses validation.</summary>
    internal static TodoTitle FromPersistence(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
