using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

public class JobApplicationFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Entreprise")]
    public Guid CompanyId { get; set; }

    [Display(Name = "Offre associée")]
    public Guid? JobOfferId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Titre du poste")]
    public string JobTitle { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date d'envoi")]
    public DateOnly? AppliedOn { get; set; }

    [Required]
    [Display(Name = "Priorité")]
    public ApplicationPriority Priority { get; set; } = ApplicationPriority.Normal;

    [Required]
    [Display(Name = "Source")]
    public JobSource Source { get; set; } = JobSource.Indeed;

    // Utilisé uniquement à la création (statut initial) : ignoré lors de la modification,
    // les changements de statut ultérieurs passent par l'action ChangeStatus dédiée.
    [Required]
    [Display(Name = "Statut initial")]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.ToApply;

    [Range(0, 10_000_000)]
    [Display(Name = "Salaire demandé")]
    public decimal? ExpectedSalary { get; set; }

    [StringLength(3)]
    [Display(Name = "Devise")]
    public string? CurrencyCode { get; set; }

    [StringLength(5000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public List<CompanyOptionViewModel> AvailableCompanies { get; set; } = [];
    public List<JobOfferOptionViewModel> AvailableJobOffers { get; set; } = [];
}

public class JobOfferOptionViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
