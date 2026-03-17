using Shared.BuildingBlocks.Domain;

namespace UserService.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid EventId, DateTime OccurredAt,
    Guid UserId, string Email, string FirstName, string LastName) : IDomainEvent;

public sealed record UserProfileUpdatedDomainEvent(
    Guid EventId, DateTime OccurredAt,
    Guid UserId, string Email, string FirstName, string LastName) : IDomainEvent;

public sealed record UserDeactivatedDomainEvent(
    Guid EventId, DateTime OccurredAt,
    Guid UserId) : IDomainEvent;
