using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Models.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerTrack.IntegrationTests;

public class ImportedEmailIsolationTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ImportedEmail BuildImport(Guid userId, string source, string externalMessageId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Source = source,
        ExternalMessageId = externalMessageId,
        ReceivedAt = DateTimeOffset.UtcNow,
        ImportedAt = DateTimeOffset.UtcNow,
        MessageType = ImportedEmailType.JobAlert,
        ProcessingStatus = EmailImportStatus.PendingReview,
        RawPayloadJson = "{}"
    };

    [Fact]
    public async Task SameUserSourceAndExternalMessageId_IsDetectedAsDuplicate()
    {
        await using ApplicationDbContext context = CreateContext();

        Guid userId = Guid.NewGuid();
        context.ImportedEmails.Add(BuildImport(userId, "gmail", "msg-123"));
        await context.SaveChangesAsync();

        // Reproduit la requête de déduplication utilisée par GmailIntegrationController
        // (section 11 de la spec : idempotence sur UserId+Source+ExternalMessageId).
        bool alreadyImported = await context.ImportedEmails
            .AnyAsync(e => e.UserId == userId && e.Source == "gmail" && e.ExternalMessageId == "msg-123");

        alreadyImported.Should().BeTrue();
    }

    [Fact]
    public async Task SameExternalMessageId_ForDifferentUsers_IsNotADuplicate()
    {
        await using ApplicationDbContext context = CreateContext();

        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        context.ImportedEmails.Add(BuildImport(userA, "gmail", "msg-123"));
        await context.SaveChangesAsync();

        bool alreadyImportedForB = await context.ImportedEmails
            .AnyAsync(e => e.UserId == userB && e.Source == "gmail" && e.ExternalMessageId == "msg-123");

        alreadyImportedForB.Should().BeFalse();
    }

    [Fact]
    public async Task UserA_CannotLoadImportedEmailBelongingToUserB()
    {
        await using ApplicationDbContext context = CreateContext();

        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        ImportedEmail importOfB = BuildImport(userB, "gmail", "msg-456");
        context.ImportedEmails.Add(importOfB);
        await context.SaveChangesAsync();

        ImportedEmail? found = await context.ImportedEmails
            .SingleOrDefaultAsync(e => e.Id == importOfB.Id && e.UserId == userA);

        found.Should().BeNull();
    }
}
