using System.ComponentModel.DataAnnotations;

namespace CareerTrack.ViewModels;

public sealed class SalaryRangeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = (JobOfferFormViewModel)validationContext.ObjectInstance;

        if (model.MinimumSalary.HasValue &&
            model.MaximumSalary.HasValue &&
            model.MinimumSalary > model.MaximumSalary)
        {
            return new ValidationResult("Le salaire minimum ne peut pas dépasser le salaire maximum.");
        }

        return ValidationResult.Success;
    }
}
