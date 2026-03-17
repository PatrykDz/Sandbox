using Shared.BuildingBlocks.Domain;
using Shared.BuildingBlocks.Result;
using TodoService.Domain.Errors;

namespace TodoService.Domain.ValueObjects;

public sealed class TodoDescription : ValueObject
{
    public const int MaxLength = 2000;

    public string Value { get; }

    private TodoDescription(string value) => Value = value;

    public static Result<TodoDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return TodoErrors.DescriptionEmpty;

        value = value.Trim();
        if (value.Length > MaxLength)
            return TodoErrors.DescriptionTooLong;

        return new TodoDescription(value);
    }

    internal static TodoDescription FromPersistence(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
