using LibraHub.Library.Domain.Reading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraHub.Library.Infrastructure.Persistence.Configurations;

public class ReadingProgressConfig : IEntityTypeConfiguration<ReadingProgress>
{
    public void Configure(EntityTypeBuilder<ReadingProgress> builder)
    {
        builder.ToTable("reading_progress");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.BookId)
            .HasColumnName("book_id")
            .IsRequired();

        builder.Property(x => x.ProgressPercentage)
            .HasColumnName("progress_percentage")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.LastPage)
            .HasColumnName("last_page");

        builder.Property(x => x.LastUpdatedAt)
            .HasColumnName("last_updated_at")
            .IsRequired();

        // Unique constraint on (UserId, BookId)
        builder.HasIndex(x => new { x.UserId, x.BookId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.BookId);
    }
}

