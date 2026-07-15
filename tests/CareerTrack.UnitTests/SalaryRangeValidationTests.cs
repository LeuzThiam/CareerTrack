using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;
using CareerTrack.ViewModels;
using FluentAssertions;
using Xunit;

namespace CareerTrack.UnitTests;

public class SalaryRangeValidationTests
{
    private static List<ValidationResult> Validate(JobOfferFormViewModel model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    private static JobOfferFormViewModel CreateValidOffer()
    {
        return new JobOfferFormViewModel
        {
            CompanyId = Guid.NewGuid(),
            Title = "Analyste de données",
            WorkMode = WorkMode.Remote,
            EmploymentType = EmploymentType.FullTime,
            Source = JobSource.Indeed,
            Status = JobOfferStatus.Open
        };
    }

    [Fact]
    public void Validate_ShouldFail_WhenMinimumSalaryExceedsMaximum()
    {
        JobOfferFormViewModel model = CreateValidOffer();
        model.MinimumSalary = 90_000;
        model.MaximumSalary = 70_000;

        List<ValidationResult> results = Validate(model);

        results.Should().Contain(r => r.ErrorMessage!.Contains("salaire minimum"));
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenMinimumSalaryIsBelowMaximum()
    {
        JobOfferFormViewModel model = CreateValidOffer();
        model.MinimumSalary = 70_000;
        model.MaximumSalary = 90_000;

        List<ValidationResult> results = Validate(model);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenSalariesAreNull()
    {
        JobOfferFormViewModel model = CreateValidOffer();

        List<ValidationResult> results = Validate(model);

        results.Should().BeEmpty();
    }
}
