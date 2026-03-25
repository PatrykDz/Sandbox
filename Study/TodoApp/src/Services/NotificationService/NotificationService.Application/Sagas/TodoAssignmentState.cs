using MassTransit;

namespace NotificationService.Application.Sagas;

/// <summary>
/// Persistent saga state for the Todo Assignment workflow.
/// Stored in PostgreSQL and correlated by TodoId.
/// </summary>
public sealed class TodoAssignmentState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }  // = TodoId
    public int Version { get; set; }
    public string CurrentState { get; set; } = default!;

    public string TodoTitle { get; set; } = default!;
    public Guid AssignedUserId { get; set; }
    public Guid? PreviousAssigneeId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? NotificationSentAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }

    // Token for the scheduled timeout message
    public Guid? NotificationTimeoutTokenId { get; set; }
}
