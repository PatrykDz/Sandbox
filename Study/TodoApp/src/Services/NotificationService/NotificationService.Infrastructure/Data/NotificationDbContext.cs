using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Sagas;

namespace NotificationService.Infrastructure.Data;

/// <summary>
/// Notification service database context.
/// Stores: saga state, MassTransit inbox/outbox tables.
///
/// Also maintains a user preferences projection populated from UserService events
/// (anti-corruption layer: NotificationService owns its own view of user preferences).
/// </summary>
public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<TodoAssignmentState> TodoAssignmentSagas => Set<TodoAssignmentState>();
    public DbSet<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MassTransit saga state map
        modelBuilder.Entity<TodoAssignmentState>(b =>
        {
            b.ToTable("saga_todo_assignment");
            b.HasKey(s => s.CorrelationId);
            b.Property(s => s.CorrelationId).HasColumnName("correlation_id");
            b.Property(s => s.CurrentState).HasColumnName("current_state").HasMaxLength(64).IsRequired();
            b.Property(s => s.Version).HasColumnName("version").IsConcurrencyToken();
            b.Property(s => s.TodoTitle).HasColumnName("todo_title").HasMaxLength(200);
            b.Property(s => s.AssignedUserId).HasColumnName("assigned_user_id");
            b.Property(s => s.PreviousAssigneeId).HasColumnName("previous_assignee_id");
            b.Property(s => s.AssignedAt).HasColumnName("assigned_at");
            b.Property(s => s.NotificationSentAt).HasColumnName("notification_sent_at");
            b.Property(s => s.FailedAt).HasColumnName("failed_at");
            b.Property(s => s.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);
            b.Property(s => s.RetryCount).HasColumnName("retry_count");
            b.Property(s => s.NotificationTimeoutTokenId).HasColumnName("notification_timeout_token_id");

            b.HasIndex(s => s.CurrentState).HasDatabaseName("ix_saga_todo_assignment_state");
            b.HasIndex(s => s.AssignedUserId).HasDatabaseName("ix_saga_todo_assignment_user");
        });

        // User notification preferences projection
        modelBuilder.Entity<UserNotificationPreference>(b =>
        {
            b.ToTable("user_notification_preferences");
            b.HasKey(p => p.UserId);
            b.Property(p => p.UserId).HasColumnName("user_id");
            b.Property(p => p.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            b.Property(p => p.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            b.Property(p => p.EmailNotificationsEnabled).HasColumnName("email_notifications_enabled");
            b.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
            b.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        });

        // MassTransit inbox (idempotent consumers) + outbox tables
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

/// <summary>
/// Local projection of user data owned by NotificationService.
/// Populated by consuming UserRegisteredEvent / UserProfileUpdatedEvent.
/// This is the Anti-Corruption Layer: NotificationService does NOT call UserService directly.
/// </summary>
public sealed class UserNotificationPreference
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
