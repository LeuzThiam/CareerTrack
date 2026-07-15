using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(application => application.Id);

        builder.Property(application => application.JobTitle)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(application => application.ExpectedSalary).HasPrecision(18, 2);
        builder.Property(application => application.CurrencyCode).HasMaxLength(3);
        builder.Property(application => application.Notes).HasMaxLength(5000);

        builder.Property(application => application.Status).HasConversion<int>();
        builder.Property(application => application.Priority).HasConversion<int>();
        builder.Property(application => application.Source).HasConversion<int>();

        builder.HasOne(application => application.User)
            .WithMany()
            .HasForeignKey(application => application.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(application => application.Company)
            .WithMany()
            .HasForeignKey(application => application.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(application => application.JobOffer)
            .WithMany()
            .HasForeignKey(application => application.JobOfferId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(application => application.StatusHistory)
            .WithOne(history => history.Application)
            .HasForeignKey(history => history.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(application => new { application.UserId, application.Status });
        builder.HasIndex(application => new { application.UserId, application.AppliedOn });

        builder.HasQueryFilter(application => application.ArchivedAt == null);
    }
}
