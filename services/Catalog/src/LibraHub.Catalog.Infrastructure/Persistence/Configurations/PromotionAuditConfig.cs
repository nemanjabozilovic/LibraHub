using LibraHub.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraHub.Catalog.Infrastructure.Persistence.Configurations;

public class PromotionAuditConfig : IEntityTypeConfiguration<PromotionAudit>
{
    public void Configure(EntityTypeBuilder<PromotionAudit> builder)
    {
        builder.ToTable("promotion_audit");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.CampaignId)
            .HasColumnName("campaign_id")
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.ActorUserId)
            .HasColumnName("actor_user_id")
            .IsRequired();

        builder.Property(a => a.AtUtc)
            .HasColumnName("at_utc")
            .IsRequired();

        builder.Property(a => a.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("jsonb");

        builder.HasIndex(a => a.CampaignId);
        builder.HasIndex(a => a.AtUtc);
    }
}
