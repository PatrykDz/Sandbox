using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoService.Domain.Entities;
using TodoService.Domain.StronglyTypedIds;
using TodoService.Domain.ValueObjects;

namespace TodoService.Infrastructure.Data.Configurations;

public sealed class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.ToTable("todos");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => TodoId.From(value));

        // Value object: TodoTitle → stored as single string column
        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(TodoTitle.MaxLength)
            .IsRequired()
            .HasConversion(
                title => title.Value,
                value => TodoTitle.FromPersistence(value));

        // Value object: TodoDescription → nullable single string column
        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(TodoDescription.MaxLength)
            .HasConversion(
                desc => desc != null ? desc.Value : null,
                value => value != null ? TodoDescription.FromPersistence(value) : null);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Strongly typed UserId → stored as nullable Guid
        builder.Property(t => t.AssignedToUserId)
            .HasColumnName("assigned_to_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : (UserId?)null);

        builder.Property(t => t.DueDate).HasColumnName("due_date");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.CompletedAt).HasColumnName("completed_at");

        builder.HasMany(t => t.Tags)
            .WithOne()
            .HasForeignKey(tag => tag.TodoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.Status).HasDatabaseName("ix_todos_status");
        builder.HasIndex(t => t.Priority).HasDatabaseName("ix_todos_priority");
        builder.HasIndex(t => t.AssignedToUserId).HasDatabaseName("ix_todos_assigned_to_user_id");
        builder.HasIndex(t => t.CreatedAt).HasDatabaseName("ix_todos_created_at");
        builder.HasIndex(t => t.DueDate).HasDatabaseName("ix_todos_due_date");

        builder.Ignore(t => t.DomainEvents);
    }
}
