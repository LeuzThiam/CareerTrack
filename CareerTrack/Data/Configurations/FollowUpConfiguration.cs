using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.ToTable("FollowUps");

        builder.HasKey(followUp => followUp.Id);

        builder.Property(followUp => followUp.Type).HasConversion<int>();
        builder.Property(followUp => followUp.Result).HasMaxLength(1000);
        builder.Property(followUp => followUp.Notes).HasMaxLength(1000);

        builder.HasOne(followUp => followUp.Application)
            .WithMany()
            .HasForeignKey(followUp => followUp.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(followUp => new { followUp.ApplicationId, followUp.ScheduledAt });
        builder.HasIndex(followUp => new { followUp.UserId, followUp.ScheduledAt });

        builder.HasQueryFilter(followUp => followUp.Application.ArchivedAt == null);
    }
}
