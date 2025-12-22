using LibraHub.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace LibraHub.Catalog.Infrastructure.Persistence.Configurations;

public class PromotionRuleConfig : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable("promotion_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.CampaignId)
            .HasColumnName("campaign_id")
            .IsRequired();

        builder.Property(r => r.DiscountType)
            .HasColumnName("discount_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.DiscountValue)
            .HasColumnName("discount_value")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3);

        builder.Property(r => r.MinPriceAfterDiscount)
            .HasColumnName("min_price_after_discount")
            .HasPrecision(18, 2);

        builder.Property(r => r.MaxDiscountAmount)
            .HasColumnName("max_discount_amount")
            .HasPrecision(18, 2);

        builder.Property(r => r.AppliesToScope)
            .HasColumnName("applies_to_scope")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.ScopeValues)
            .HasColumnName("scope_value")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null),
                new ValueComparer<List<string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()))
            .HasColumnType("jsonb");

        builder.Property(r => r.Exclusions)
            .HasColumnName("exclusions")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null),
                new ValueComparer<List<Guid>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()))
            .HasColumnType("jsonb");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
