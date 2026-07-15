using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
    {
        builder.ToTable("ApplicationStatusHistories");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Comment).HasMaxLength(1000);
        builder.Property(history => history.PreviousStatus).HasConversion<int?>();
        builder.Property(history => history.NewStatus).HasConversion<int>();

        builder.HasIndex(history => history.ApplicationId);

        // Aligné sur le filtre de JobApplication (ArchivedAt == null) pour éviter
        // l'avertissement EF Core sur des filtres incohérents entre entités liées.
        builder.HasQueryFilter(history => history.Application.ArchivedAt == null);
    }
}
