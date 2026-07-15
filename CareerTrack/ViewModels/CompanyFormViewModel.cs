using System.ComponentModel.DataAnnotations;

namespace CareerTrack.ViewModels;

public class CompanyFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Secteur")]
    public string? Industry { get; set; }

    [StringLength(300)]
    [Display(Name = "Site web")]
    [Url]
    public string? WebsiteUrl { get; set; }

    [StringLength(150)]
    [Display(Name = "Ville")]
    public string? City { get; set; }

    [StringLength(150)]
    [Display(Name = "Province")]
    public string? Province { get; set; }

    [StringLength(150)]
    [Display(Name = "Pays")]
    public string? Country { get; set; }

    [StringLength(2000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
