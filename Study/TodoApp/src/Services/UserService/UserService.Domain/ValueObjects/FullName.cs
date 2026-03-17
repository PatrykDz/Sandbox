using Shared.BuildingBlocks.Domain;
using Shared.BuildingBlocks.Result;
using UserService.Domain.Errors;

namespace UserService.Domain.ValueObjects;

public sealed class FullName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string DisplayName => $"{FirstName} {LastName}";

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Result<FullName> Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) return UserErrors.FirstNameRequired;
        if (string.IsNullOrWhiteSpace(lastName)) return UserErrors.LastNameRequired;
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        if (firstName.Length > 100) return UserErrors.FirstNameTooLong;
        if (lastName.Length > 100) return UserErrors.LastNameTooLong;
        return new FullName(firstName, lastName);
    }

    internal static FullName FromPersistence(string firstName, string lastName) => new(firstName, lastName);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => DisplayName;
}
