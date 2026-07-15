using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

public class ChangeStatusViewModel
{
    public Guid ApplicationId { get; set; }

    [Required]
    [Display(Name = "Nouveau statut")]
    public ApplicationStatus NewStatus { get; set; }

    [StringLength(1000)]
    [Display(Name = "Commentaire")]
    public string? Comment { get; set; }

    public List<ApplicationStatus> AvailableStatuses { get; set; } = [];
}
