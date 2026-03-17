using Shared.BuildingBlocks.Result;

namespace UserService.Domain.Errors;

public static class UserErrors
{
    public static readonly Error EmailRequired = Error.Validation("User.EmailRequired", "Email is required.");
    public static readonly Error EmailTooLong = Error.Validation("User.EmailTooLong", "Email must not exceed 320 characters.");
    public static readonly Error EmailInvalid = Error.Validation("User.EmailInvalid", "Email format is invalid.");
    public static readonly Error FirstNameRequired = Error.Validation("User.FirstNameRequired", "First name is required.");
    public static readonly Error LastNameRequired = Error.Validation("User.LastNameRequired", "Last name is required.");
    public static readonly Error FirstNameTooLong = Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters.");
    public static readonly Error LastNameTooLong = Error.Validation("User.LastNameTooLong", "Last name must not exceed 100 characters.");
    public static readonly Error AlreadyDeactivated = Error.Conflict("User.AlreadyDeactivated", "User is already deactivated.");
    public static readonly Error EmailAlreadyInUse = Error.Conflict("User.EmailAlreadyInUse", "This email address is already in use.");
    public static Error NotFound(Guid id) => Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");
}
