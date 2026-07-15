using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class JobOfferConfiguration : IEntityTypeConfiguration<JobOffer>
{
    public void Configure(EntityTypeBuilder<JobOffer> builder)
    {
        builder.ToTable("JobOffers");

        builder.HasKey(offer => offer.Id);

        builder.Property(offer => offer.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(offer => offer.Description).HasMaxLength(4000);
        builder.Property(offer => offer.Location).HasMaxLength(200);
        builder.Property(offer => offer.SourceUrl).HasMaxLength(500);
        builder.Property(offer => offer.CurrencyCode).HasMaxLength(3);

        builder.Property(offer => offer.MinimumSalary).HasPrecision(18, 2);
        builder.Property(offer => offer.MaximumSalary).HasPrecision(18, 2);

        builder.Property(offer => offer.WorkMode).HasConversion<int>();
        builder.Property(offer => offer.EmploymentType).HasConversion<int>();
        builder.Property(offer => offer.Source).HasConversion<int>();
        builder.Property(offer => offer.Status).HasConversion<int>();

        builder.HasOne(offer => offer.User)
            .WithMany()
            .HasForeignKey(offer => offer.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(offer => offer.Company)
            .WithMany()
            .HasForeignKey(offer => offer.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(offer => new { offer.UserId, offer.ClosingDate });

        // Règle 18 : salaire minimum <= salaire maximum, imposée au niveau base en plus du ViewModel
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_JobOffers_SalaryRange",
            "[MinimumSalary] IS NULL OR [MaximumSalary] IS NULL OR [MinimumSalary] <= [MaximumSalary]"));

        builder.HasQueryFilter(offer => offer.ArchivedAt == null);
    }
}
