using CareerTrack.Data;
using CareerTrack.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerTrack.IntegrationTests;

public class UserIsolationTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task UserA_CannotLoadCompanyBelongingToUserB()
    {
        await using ApplicationDbContext context = CreateContext();

        Guid userAId = Guid.NewGuid();
        Guid userBId = Guid.NewGuid();

        var companyOfB = new Company
        {
            Id = Guid.NewGuid(),
            UserId = userBId,
            Name = "Entreprise de B",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Companies.Add(companyOfB);
        await context.SaveChangesAsync();

        // Reproduit exactement le pattern utilisé dans les contrôleurs : filtrer par UserId,
        // jamais uniquement par Id (Règle 1).
        Company? found = await context.Companies
            .SingleOrDefaultAsync(c => c.Id == companyOfB.Id && c.UserId == userAId);

        found.Should().BeNull();
    }

    [Fact]
    public async Task UserA_CanLoadTheirOwnCompany()
    {
        await using ApplicationDbContext context = CreateContext();

        Guid userAId = Guid.NewGuid();

        var companyOfA = new Company
        {
            Id = Guid.NewGuid(),
            UserId = userAId,
            Name = "Entreprise de A",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Companies.Add(companyOfA);
        await context.SaveChangesAsync();

        Company? found = await context.Companies
            .SingleOrDefaultAsync(c => c.Id == companyOfA.Id && c.UserId == userAId);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Entreprise de A");
    }

    [Fact]
    public async Task ArchivedCompany_IsExcludedFromDefaultQueries()
    {
        await using ApplicationDbContext context = CreateContext();

        Guid userId = Guid.NewGuid();

        var archivedCompany = new Company
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Entreprise archivée",
            ArchivedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Companies.Add(archivedCompany);
        await context.SaveChangesAsync();

        // Le filtre global (Règle 17 : suppression logique) doit exclure l'entreprise
        // archivée sans qu'on ait besoin d'ajouter ArchivedAt == null explicitement.
        List<Company> visibleCompanies = await context.Companies
            .Where(c => c.UserId == userId)
            .ToListAsync();

        visibleCompanies.Should().BeEmpty();
    }
}
