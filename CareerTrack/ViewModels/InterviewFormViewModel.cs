using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

public class InterviewFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [Display(Name = "Type")]
    public InterviewType Type { get; set; } = InterviewType.ScreeningCall;

    [Required]
    [Display(Name = "Date et heure")]
    public DateTimeOffset ScheduledAt { get; set; } = DateTimeOffset.UtcNow.AddDays(3);

    [Range(1, 600)]
    [Display(Name = "Durée (minutes)")]
    public int? DurationMinutes { get; set; }

    [StringLength(300)]
    [Display(Name = "Lieu")]
    public string? Location { get; set; }

    [StringLength(500)]
    [Display(Name = "Lien de visioconférence")]
    public string? MeetingUrl { get; set; }

    [StringLength(2000)]
    [Display(Name = "Notes de préparation")]
    public string? PreparationNotes { get; set; }
}

public class CompleteInterviewViewModel
{
    [Required]
    public Guid InterviewId { get; set; }

    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [Display(Name = "Statut final")]
    public InterviewStatus Status { get; set; } = InterviewStatus.Completed;

    [StringLength(2000)]
    [Display(Name = "Compte rendu")]
    public string? DebriefNotes { get; set; }

    [StringLength(500)]
    [Display(Name = "Résultat")]
    public string? Result { get; set; }
}
