using AuditService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditService.Infrastructure.Data.Configurations;

public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("audit_records");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.EventType).HasColumnName("event_type").HasMaxLength(200).IsRequired();
        builder.Property(a => a.EventVersion).HasColumnName("event_version").HasMaxLength(20).IsRequired();
        builder.Property(a => a.AggregateType).HasColumnName("aggregate_type").HasMaxLength(100).IsRequired();
        builder.Property(a => a.AggregateId).HasColumnName("aggregate_id");
        builder.Property(a => a.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(a => a.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(a => a.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(a => a.ServiceSource).HasColumnName("service_source").HasMaxLength(100).IsRequired();

        builder.HasIndex(a => a.AggregateId).HasDatabaseName("ix_audit_records_aggregate_id");
        builder.HasIndex(a => a.EventType).HasDatabaseName("ix_audit_records_event_type");
        builder.HasIndex(a => a.OccurredAt).HasDatabaseName("ix_audit_records_occurred_at");
        builder.HasIndex(a => a.AggregateId).HasDatabaseName("ix_audit_records_aggregate_id_occurred_at");
    }
}
