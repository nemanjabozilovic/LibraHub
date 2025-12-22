using LibraHub.Identity.Domain.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraHub.Identity.Infrastructure.Persistence.Configurations;

public class EmailVerificationTokenConfig : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(evt => evt.Id);

        builder.Property(evt => evt.Id)
            .HasColumnName("id");

        builder.Property(evt => evt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(evt => evt.UserId);

        builder.Property(evt => evt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(evt => evt.Token)
            .IsUnique();

        builder.Property(evt => evt.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(evt => evt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(evt => evt.UsedAt)
            .HasColumnName("used_at");
    }
}
