using Shared.BuildingBlocks.Domain;
using TodoService.Domain.Enums;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Domain.Events;

public sealed record TodoCreatedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId,
    string Title,
    string? Description,
    TodoPriority Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate) : IDomainEvent;

public sealed record TodoUpdatedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId,
    string Title,
    string? Description,
    TodoPriority Priority,
    DateTime? DueDate) : IDomainEvent;

public sealed record TodoCompletedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId,
    string Title,
    Guid? AssignedToUserId) : IDomainEvent;

public sealed record TodoCancelledDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId,
    string Reason) : IDomainEvent;

public sealed record TodoDeletedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId) : IDomainEvent;

public sealed record TodoAssignedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId,
    string Title,
    UserId AssignedToUserId,
    UserId? PreviousAssigneeId) : IDomainEvent;

public sealed record TodoStatusChangedDomainEvent(
    Guid EventId,
    DateTime OccurredAt,
    TodoId TodoId,
    TodoStatus From,
    TodoStatus To) : IDomainEvent;
