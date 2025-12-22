using LibraHub.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraHub.Catalog.Infrastructure.Persistence.Configurations;

public class PromotionCampaignConfig : IEntityTypeConfiguration<PromotionCampaign>
{
    public void Configure(EntityTypeBuilder<PromotionCampaign> builder)
    {
        builder.ToTable("promotion_campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.StartsAtUtc)
            .HasColumnName("starts_at_utc")
            .IsRequired();

        builder.Property(c => c.EndsAtUtc)
            .HasColumnName("ends_at_utc")
            .IsRequired();

        builder.Property(c => c.StackingPolicy)
            .HasColumnName("stacking_policy")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(c => c.Rules)
            .WithOne()
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
