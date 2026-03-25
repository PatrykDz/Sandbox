namespace TodoService.Domain.StronglyTypedIds;

/// <summary>
/// Strongly typed ID for the Todo aggregate. Prevents accidental mixing of Guid identifiers.
/// </summary>
public readonly record struct TodoId(Guid Value)
{
    public static TodoId New() => new(Guid.NewGuid());
    public static TodoId From(Guid value) => new(value);
    public static bool TryParse(string input, out TodoId result)
    {
        if (Guid.TryParse(input, out var guid))
        {
            result = new TodoId(guid);
            return true;
        }
        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
    public static implicit operator Guid(TodoId id) => id.Value;
}
