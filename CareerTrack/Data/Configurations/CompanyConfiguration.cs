using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(company => company.Industry).HasMaxLength(150);
        builder.Property(company => company.WebsiteUrl).HasMaxLength(300);
        builder.Property(company => company.City).HasMaxLength(150);
        builder.Property(company => company.Province).HasMaxLength(150);
        builder.Property(company => company.Country).HasMaxLength(150);
        builder.Property(company => company.Notes).HasMaxLength(2000);

        builder.HasOne(company => company.User)
            .WithMany()
            .HasForeignKey(company => company.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(company => new { company.UserId, company.Name });

        // Filtre global : les entreprises archivées sont exclues de toutes les requêtes par défaut
        // (suppression logique, voir Règle 17 du cahier des charges)
        builder.HasQueryFilter(company => company.ArchivedAt == null);
    }
}
