using Shared.BuildingBlocks.Domain;
using TodoService.Domain.Enums;

namespace TodoService.Domain.Events;

public sealed record TodoCreatedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid TodoId,
    string Title,
    string? Description,
    TodoPriority Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate) : IDomainEvent;

public sealed record TodoUpdatedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid TodoId,
    string Title,
    string? Description,
    TodoPriority Priority,
    DateTime? DueDate) : IDomainEvent;

public sealed record TodoCompletedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid TodoId,
    string Title,
    Guid? AssignedToUserId) : IDomainEvent;

public sealed record TodoDeletedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid TodoId) : IDomainEvent;

public sealed record TodoCancelledDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid TodoId,
    string Reason) : IDomainEvent;
