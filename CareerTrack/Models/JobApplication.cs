using CareerTrack.Exceptions;
using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

public class JobApplication
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? JobOfferId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateOnly? AppliedOn { get; set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.ToApply;
    public ApplicationPriority Priority { get; set; }
    public JobSource Source { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ApplicationUser User { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public JobOffer? JobOffer { get; set; }
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();

    // Point d'entrée unique pour changer le statut : garantit qu'aucun changement
    // n'échappe à la validation de transition (Règle 9). Retourne la nouvelle entrée
    // d'historique plutôt que de l'ajouter à la collection StatusHistory : quand cette
    // collection a été chargée via Include() sur une entité existante, EF Core peut
    // confondre l'ajout avec une modification de l'entrée déjà suivie et générer un
    // UPDATE au lieu d'un INSERT. L'appelant doit l'ajouter explicitement au DbSet.
    public ApplicationStatusHistory ChangeStatus(ApplicationStatus newStatus, string? comment, DateTimeOffset changedAt)
    {
        if (!ApplicationStatusTransitions.IsAllowed(Status, newStatus))
        {
            throw new InvalidStatusTransitionException(Status, newStatus);
        }

        var historyEntry = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = Id,
            PreviousStatus = Status,
            NewStatus = newStatus,
            Comment = comment,
            ChangedAt = changedAt
        };

        Status = newStatus;
        UpdatedAt = changedAt;

        return historyEntry;
    }

    // Utilisé uniquement à la création, pour fixer le statut initial et son entrée d'historique.
    public void InitializeStatus(ApplicationStatus initialStatus, DateTimeOffset createdAt)
    {
        Status = initialStatus;
        StatusHistory.Add(new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = Id,
            PreviousStatus = null,
            NewStatus = initialStatus,
            Comment = "Création de la candidature",
            ChangedAt = createdAt
        });
    }
}
