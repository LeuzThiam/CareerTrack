using System.ComponentModel.DataAnnotations;

namespace CareerTrack.ViewModels;

public class ContactFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Entreprise")]
    public Guid CompanyId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Prénom")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Nom")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Poste")]
    public string? JobTitle { get; set; }

    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Courriel")]
    public string? Email { get; set; }

    [Phone]
    [StringLength(30)]
    [Display(Name = "Téléphone")]
    public string? PhoneNumber { get; set; }

    [StringLength(300)]
    [Display(Name = "LinkedIn")]
    public string? LinkedInUrl { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public List<CompanyOptionViewModel> AvailableCompanies { get; set; } = [];
}
