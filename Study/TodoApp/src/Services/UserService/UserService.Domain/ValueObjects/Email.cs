using Shared.BuildingBlocks.Domain;
using Shared.BuildingBlocks.Result;
using System.Text.RegularExpressions;
using UserService.Domain.Errors;

namespace UserService.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public const int MaxLength = 320;
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return UserErrors.EmailRequired;
        value = value.Trim().ToLowerInvariant();
        if (value.Length > MaxLength)
            return UserErrors.EmailTooLong;
        if (!EmailRegex.IsMatch(value))
            return UserErrors.EmailInvalid;
        return new Email(value);
    }

    internal static Email FromPersistence(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
