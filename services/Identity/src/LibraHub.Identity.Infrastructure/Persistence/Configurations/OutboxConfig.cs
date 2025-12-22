using LibraHub.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraHub.Identity.Infrastructure.Persistence.Configurations;

public class OutboxConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(om => om.Id);

        builder.Property(om => om.Id)
            .HasColumnName("id");

        builder.Property(om => om.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(om => new { om.ProcessedAt, om.CreatedAt });

        builder.Property(om => om.Payload)
            .HasColumnName("payload")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(om => om.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(om => om.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(om => om.Error)
            .HasColumnName("error")
            .HasMaxLength(1000);
    }
}
