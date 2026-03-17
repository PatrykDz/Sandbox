namespace TodoService.Domain.StronglyTypedIds;

/// <summary>
/// External reference to a User identity from the UserService bounded context.
/// TodoService does NOT own User data — this is just a foreign key by convention.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(UserId id) => id.Value;
}
