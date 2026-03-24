using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoService.Domain.Entities;
using TodoService.Domain.StronglyTypedIds;

namespace TodoService.Infrastructure.Data.Configurations;

internal sealed class TodoHistoryEntryConfiguration : IEntityTypeConfiguration<TodoHistoryEntry>
{
    public void Configure(EntityTypeBuilder<TodoHistoryEntry> builder)
    {
        builder.ToTable("todo_history_entries");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(h => h.TodoId)
            .HasColumnName("todo_id")
            .HasConversion(id => id.Value, value => TodoId.From(value))
            .IsRequired();

        builder.Property(h => h.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.PreviousState)
            .HasColumnName("previous_state")
            .HasColumnType("text");

        builder.Property(h => h.CurrentState)
            .HasColumnName("current_state")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(h => h.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

        builder.Property(h => h.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        // Index for fast lookup by todo_id ordered by time
        builder.HasIndex(h => new { h.TodoId, h.OccurredAt })
            .HasDatabaseName("ix_todo_history_entries_todo_id_occurred_at");

        // Index for event type filtering
        builder.HasIndex(h => h.EventType)
            .HasDatabaseName("ix_todo_history_entries_event_type");
    }
}
