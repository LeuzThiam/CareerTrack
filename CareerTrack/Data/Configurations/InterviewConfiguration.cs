using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("Interviews");

        builder.HasKey(interview => interview.Id);

        builder.Property(interview => interview.Type).HasConversion<int>();
        builder.Property(interview => interview.Status).HasConversion<int>();
        builder.Property(interview => interview.Location).HasMaxLength(300);
        builder.Property(interview => interview.MeetingUrl).HasMaxLength(500);
        builder.Property(interview => interview.PreparationNotes).HasMaxLength(2000);
        builder.Property(interview => interview.DebriefNotes).HasMaxLength(2000);
        builder.Property(interview => interview.Result).HasMaxLength(500);

        builder.HasOne(interview => interview.Application)
            .WithMany()
            .HasForeignKey(interview => interview.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(interview => new { interview.ApplicationId, interview.ScheduledAt });
        builder.HasIndex(interview => new { interview.UserId, interview.ScheduledAt });

        builder.HasQueryFilter(interview => interview.Application.ArchivedAt == null);
    }
}
