using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(contact => contact.Id);

        builder.Property(contact => contact.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(contact => contact.LastName).HasMaxLength(100).IsRequired();
        builder.Property(contact => contact.JobTitle).HasMaxLength(150);
        builder.Property(contact => contact.Email).HasMaxLength(256);
        builder.Property(contact => contact.PhoneNumber).HasMaxLength(30);
        builder.Property(contact => contact.LinkedInUrl).HasMaxLength(300);
        builder.Property(contact => contact.Notes).HasMaxLength(1000);

        builder.HasOne(contact => contact.User)
            .WithMany()
            .HasForeignKey(contact => contact.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(contact => contact.Company)
            .WithMany()
            .HasForeignKey(contact => contact.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(contact => new { contact.UserId, contact.CompanyId });

        // Aligné sur le filtre de Company (ArchivedAt == null) pour éviter
        // l'avertissement EF Core sur des filtres incohérents entre entités liées.
        builder.HasQueryFilter(contact => contact.Company.ArchivedAt == null);
    }
}
