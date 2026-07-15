using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class ApplicationDocumentConfiguration : IEntityTypeConfiguration<ApplicationDocument>
{
    public void Configure(EntityTypeBuilder<ApplicationDocument> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(document => document.StoredFileName).HasMaxLength(260).IsRequired();
        builder.Property(document => document.Type).HasConversion<int>();
        builder.Property(document => document.ContentType).HasMaxLength(150).IsRequired();

        builder.HasOne(document => document.Application)
            .WithMany()
            .HasForeignKey(document => document.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(document => document.ApplicationId);

        builder.HasQueryFilter(document => document.Application.ArchivedAt == null);
    }
}
