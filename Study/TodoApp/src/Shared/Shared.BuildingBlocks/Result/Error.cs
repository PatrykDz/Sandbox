namespace Shared.BuildingBlocks.Result;

/// <summary>
/// Represents a structured error with a code and human-readable description.
/// Prefer using static factory methods over direct construction.
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);

    public static Error Unauthorized(string code, string description)
        => new(code, description, ErrorType.Unauthorized);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4
}
