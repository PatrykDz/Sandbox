using Shared.BuildingBlocks.Result;

namespace TodoService.Domain.Errors;

/// <summary>
/// Centralised catalog of domain errors. Keeps error codes consistent across the bounded context.
/// </summary>
public static class TodoErrors
{
    // Title
    public static readonly Error TitleRequired =
        Error.Validation("Todo.TitleRequired", "Title is required.");
    public static readonly Error TitleTooLong =
        Error.Validation("Todo.TitleTooLong", $"Title must not exceed {200} characters.");

    // Description
    public static readonly Error DescriptionEmpty =
        Error.Validation("Todo.DescriptionEmpty", "Description cannot be empty whitespace.");
    public static readonly Error DescriptionTooLong =
        Error.Validation("Todo.DescriptionTooLong", $"Description must not exceed {2000} characters.");

    // State transitions
    public static readonly Error AlreadyCompleted =
        Error.Conflict("Todo.AlreadyCompleted", "This todo is already completed.");
    public static readonly Error AlreadyCancelled =
        Error.Conflict("Todo.AlreadyCancelled", "This todo is already cancelled.");
    public static readonly Error CannotUpdateTerminated =
        Error.Conflict("Todo.CannotUpdateTerminated", "Cannot update a completed or cancelled todo.");

    // Not found
    public static Error NotFound(Guid id) =>
        Error.NotFound("Todo.NotFound", $"Todo with id '{id}' was not found.");

    // Assignment
    public static readonly Error AssignedToSameUser =
        Error.Conflict("Todo.AssignedToSameUser", "Todo is already assigned to this user.");

    // Invalid transition
    public static Error InvalidTransition(string from, string to) =>
        Error.Conflict("Todo.InvalidTransition", $"Cannot transition from '{from}' to '{to}'.");
}
