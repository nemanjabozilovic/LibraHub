using LibraHub.BuildingBlocks.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraHub.Identity.Infrastructure.Persistence.Configurations;

public class InboxConfig : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("processed_messages");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasColumnName("id");

        builder.Property(pm => pm.MessageId)
            .HasColumnName("message_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(pm => pm.MessageId)
            .IsUnique();

        builder.Property(pm => pm.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pm => pm.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();
    }
}
