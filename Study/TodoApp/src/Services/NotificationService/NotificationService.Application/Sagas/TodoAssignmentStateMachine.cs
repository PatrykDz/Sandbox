using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;

namespace NotificationService.Application.Sagas;

/// <summary>
/// MassTransit Saga (process manager) that orchestrates the Todo Assignment notification workflow.
///
/// States:
///   Initial → WaitingForNotification → (NotificationSent → Completed) | (Timeout → Failed)
///
/// On assignment:
///   1. Record the assignment
///   2. Schedule a 5-minute timeout
///   3. Transition to WaitingForNotification
///
/// On NotificationSentEvent (correlation by TodoId):
///   1. Cancel the timeout
///   2. Record completion
///   3. Transition to Completed
///
/// On timeout:
///   1. Log the failure for observability
///   2. Transition to Failed (could trigger compensating actions or alerting)
/// </summary>
public sealed class TodoAssignmentStateMachine : MassTransitStateMachine<TodoAssignmentState>
{
    // States
    public State WaitingForNotification { get; private set; } = default!;
    public State Completed { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    // Events
    public Event<TodoAssignedEvent> TodoAssigned { get; private set; } = default!;
    public Event<NotificationSentEvent> NotificationSent { get; private set; } = default!;

    // Scheduled timeout
    public Schedule<TodoAssignmentState, TodoAssignmentNotificationTimeout> NotificationTimeout { get; private set; } = default!;

    public TodoAssignmentStateMachine(ILogger<TodoAssignmentStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        // Correlate by TodoId
        Event(() => TodoAssigned,
            x => x.CorrelateById(m => m.Message.TodoId));

        Event(() => NotificationSent,
            x => x.CorrelateById(m => m.Message.TodoId));

        Schedule(() => NotificationTimeout,
            x => x.NotificationTimeoutTokenId,
            s =>
            {
                s.Delay = TimeSpan.FromMinutes(5);
                s.Received = r => r.CorrelateById(m => m.Message.TodoId);
            });

        // ─── Initially ────────────────────────────────────────────────────
        Initially(
            When(TodoAssigned)
                .Then(ctx =>
                {
                    ctx.Saga.TodoTitle = ctx.Message.Title;
                    ctx.Saga.AssignedUserId = ctx.Message.AssignedToUserId;
                    ctx.Saga.PreviousAssigneeId = ctx.Message.PreviousAssigneeId;
                    ctx.Saga.AssignedAt = ctx.Message.AssignedAt;
                    logger.LogInformation(
                        "Saga started for TodoId={TodoId}, AssignedTo={UserId}",
                        ctx.Saga.CorrelationId, ctx.Saga.AssignedUserId);
                })
                .Schedule(NotificationTimeout,
                    ctx => new TodoAssignmentNotificationTimeout { TodoId = ctx.Saga.CorrelationId })
                .TransitionTo(WaitingForNotification));

        // ─── WaitingForNotification ────────────────────────────────────────
        During(WaitingForNotification,

            When(NotificationSent)
                .Then(ctx =>
                {
                    ctx.Saga.NotificationSentAt = ctx.Message.SentAt;
                    logger.LogInformation(
                        "Notification sent for TodoId={TodoId} via {Channel}",
                        ctx.Saga.CorrelationId, ctx.Message.Channel);
                })
                .Unschedule(NotificationTimeout)
                .TransitionTo(Completed),

            When(NotificationTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    ctx.Saga.FailureReason = "Notification not confirmed within timeout window.";
                    logger.LogWarning(
                        "Assignment notification timed out for TodoId={TodoId}, AssignedUser={UserId}",
                        ctx.Saga.CorrelationId, ctx.Saga.AssignedUserId);
                })
                .TransitionTo(Failed));

        // ─── Terminal states ───────────────────────────────────────────────
        // Keep completed sagas for audit; set DeletionThreshold if cleanup is needed
        SetCompletedWhenFinalized();
    }
}
