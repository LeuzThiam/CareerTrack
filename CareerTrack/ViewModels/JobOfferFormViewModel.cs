using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

[SalaryRange]
public class JobOfferFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Entreprise")]
    public Guid CompanyId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Titre du poste")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(200)]
    [Display(Name = "Lieu")]
    public string? Location { get; set; }

    [Required]
    [Display(Name = "Mode de travail")]
    public WorkMode WorkMode { get; set; } = WorkMode.Unspecified;

    [Required]
    [Display(Name = "Type de contrat")]
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

    [Required]
    [Display(Name = "Source")]
    public JobSource Source { get; set; } = JobSource.Indeed;

    [StringLength(500)]
    [Display(Name = "Lien de l'offre")]
    [Url]
    public string? SourceUrl { get; set; }

    [Range(0, 10_000_000)]
    [Display(Name = "Salaire minimum")]
    public decimal? MinimumSalary { get; set; }

    [Range(0, 10_000_000)]
    [Display(Name = "Salaire maximum")]
    public decimal? MaximumSalary { get; set; }

    [StringLength(3)]
    [Display(Name = "Devise")]
    public string? CurrencyCode { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date de publication")]
    public DateOnly? PublicationDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date limite")]
    public DateOnly? ClosingDate { get; set; }

    [Required]
    [Display(Name = "Statut")]
    public JobOfferStatus Status { get; set; } = JobOfferStatus.Open;

    public List<CompanyOptionViewModel> AvailableCompanies { get; set; } = [];
}

public class CompanyOptionViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
