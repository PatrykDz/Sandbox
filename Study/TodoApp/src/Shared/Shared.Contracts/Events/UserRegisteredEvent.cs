namespace Shared.Contracts.Events;

public record UserRegisteredEvent(
    Guid EventId,
    DateTime OccurredAt,
    string EventVersion,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime RegisteredAt)
{
    public UserRegisteredEvent(Guid userId, string email, string firstName, string lastName)
        : this(Guid.NewGuid(), DateTime.UtcNow, "1.0", userId, email, firstName, lastName, DateTime.UtcNow) { }
}
