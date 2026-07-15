using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

public class FollowUpFormViewModel
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [Display(Name = "Type")]
    public FollowUpType Type { get; set; } = FollowUpType.Email;

    [Required]
    [Display(Name = "Date prévue")]
    public DateTimeOffset ScheduledAt { get; set; } = DateTimeOffset.UtcNow.AddDays(7);

    [StringLength(1000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

public class CompleteFollowUpViewModel
{
    [Required]
    public Guid FollowUpId { get; set; }

    [Required]
    public Guid ApplicationId { get; set; }

    [StringLength(1000)]
    [Display(Name = "Résultat")]
    public string? Result { get; set; }
}
