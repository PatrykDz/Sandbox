namespace TodoService.Domain.Exceptions;

public class TodoDomainException : Exception
{
    public TodoDomainException(string message) : base(message) { }

    public static TodoDomainException NotFound(Guid id) =>
        new($"Todo with id '{id}' was not found.");

    public static TodoDomainException AlreadyCompleted(Guid id) =>
        new($"Todo with id '{id}' is already completed.");

    public static TodoDomainException AlreadyCancelled(Guid id) =>
        new($"Todo with id '{id}' is already cancelled.");

    public static TodoDomainException InvalidTransition(string from, string to) =>
        new($"Cannot transition from '{from}' to '{to}'.");
}
