using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerTrack.Data.Configurations;

public class ImportedEmailConfiguration : IEntityTypeConfiguration<ImportedEmail>
{
    public void Configure(EntityTypeBuilder<ImportedEmail> builder)
    {
        builder.ToTable("ImportedEmails");

        builder.HasKey(email => email.Id);

        builder.Property(email => email.Source).HasMaxLength(50).IsRequired();
        builder.Property(email => email.ExternalMessageId).HasMaxLength(200).IsRequired();
        builder.Property(email => email.ExternalThreadId).HasMaxLength(200);
        builder.Property(email => email.SenderName).HasMaxLength(200);
        builder.Property(email => email.SenderEmail).HasMaxLength(256);
        builder.Property(email => email.Subject).HasMaxLength(500);
        builder.Property(email => email.Summary).HasMaxLength(2000);
        builder.Property(email => email.Language).HasMaxLength(10);
        builder.Property(email => email.CompanyNameDetected).HasMaxLength(200);
        builder.Property(email => email.JobTitleDetected).HasMaxLength(200);
        builder.Property(email => email.RecruiterNameDetected).HasMaxLength(200);
        builder.Property(email => email.RecruiterEmailDetected).HasMaxLength(256);
        builder.Property(email => email.MeetingUrlDetected).HasMaxLength(500);
        builder.Property(email => email.SuggestedApplicationStatus).HasMaxLength(100);
        builder.Property(email => email.ImportantExcerpt).HasMaxLength(1000);
        builder.Property(email => email.ProcessingError).HasMaxLength(2000);
        builder.Property(email => email.RawPayloadJson).HasColumnType("nvarchar(max)");

        builder.Property(email => email.MessageType).HasConversion<int>();
        builder.Property(email => email.SuggestedAction).HasConversion<int?>();
        builder.Property(email => email.ProcessingStatus).HasConversion<int>();

        builder.HasOne(email => email.User)
            .WithMany()
            .HasForeignKey(email => email.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(email => email.Application)
            .WithMany()
            .HasForeignKey(email => email.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Idempotence (section 11) : n8n peut renvoyer le même message (nouvelle tentative,
        // erreur réseau, exécution manuelle) — cet index empêche un double import.
        builder.HasIndex(email => new { email.UserId, email.Source, email.ExternalMessageId })
            .IsUnique();

        builder.HasIndex(email => new { email.UserId, email.ProcessingStatus });
    }
}
