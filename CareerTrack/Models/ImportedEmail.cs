using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

// Représente un courriel analysé par n8n et poussé vers CareerTrack. Jamais créé
// directement comme modification d'une candidature — reste une proposition à valider
// par l'utilisateur (section 5 de la spec d'intégration : validation humaine en V1).
public class ImportedEmail
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Source { get; set; } = string.Empty;
    public string ExternalMessageId { get; set; } = string.Empty;
    public string? ExternalThreadId { get; set; }

    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public string? Subject { get; set; }
    public string? Summary { get; set; }
    public string? Language { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset ImportedAt { get; set; }

    public ImportedEmailType MessageType { get; set; }
    public bool IsJobRelated { get; set; }
    public double ConfidenceScore { get; set; }
    public bool RequiresReview { get; set; }

    public string? CompanyNameDetected { get; set; }
    public string? JobTitleDetected { get; set; }
    public string? RecruiterNameDetected { get; set; }
    public string? RecruiterEmailDetected { get; set; }
    public DateTimeOffset? InterviewDateDetected { get; set; }
    public string? MeetingUrlDetected { get; set; }

    public SuggestedActionType? SuggestedAction { get; set; }
    public string? SuggestedApplicationStatus { get; set; }
    public string? ImportantExcerpt { get; set; }

    public Guid? ApplicationId { get; set; }

    public EmailImportStatus ProcessingStatus { get; set; } = EmailImportStatus.Received;
    public string? ProcessingError { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    // JSON brut reçu de n8n (pas le corps complet du courriel) : sert à auditer/rejouer
    // un import en cas d'erreur ou d'évolution du schéma, sans conserver le contenu intégral.
    public string RawPayloadJson { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
    public JobApplication? Application { get; set; }
}
