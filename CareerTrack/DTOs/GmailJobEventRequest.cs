using System.Text.Json.Serialization;

namespace CareerTrack.DTOs;

// Reçu depuis n8n (section 7 de la spec d'intégration Gmail). Les enums C# ne sont
// jamais déduits automatiquement du JSON : chaque code (ex. "INTERVIEW_INVITATION")
// est reçu comme string puis mappé explicitement dans GmailJobEventMapper, pour que
// la correspondance reste auditable et qu'un code inconnu produise une erreur claire
// plutôt qu'une conversion silencieuse.
public sealed class GmailJobEventRequest
{
    public string SchemaVersion { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ExternalMessageId { get; set; } = string.Empty;
    public string? ExternalThreadId { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }

    public GmailSenderInfo? Sender { get; set; }
    public GmailEmailInfo? Email { get; set; }
    public GmailClassificationInfo Classification { get; set; } = new();
    public GmailJobInfo? Job { get; set; }
    public GmailRecruiterInfo? Recruiter { get; set; }
    public GmailInterviewInfo? Interview { get; set; }
    public GmailSuggestedActionInfo? SuggestedAction { get; set; }
    public GmailEvidenceInfo? Evidence { get; set; }
}

public sealed class GmailSenderInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public sealed class GmailEmailInfo
{
    public string? Subject { get; set; }
    public string? Language { get; set; }
    public string? Summary { get; set; }
}

public sealed class GmailClassificationInfo
{
    public bool IsJobRelated { get; set; }

    // Reçu en string (ex. "INTERVIEW_INVITATION") — voir GmailJobEventMapper pour le mapping vers ImportedEmailType.
    public string MessageType { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }
    public bool RequiresReview { get; set; }
}

public sealed class GmailJobInfo
{
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public string? SourceUrl { get; set; }
}

public sealed class GmailRecruiterInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public sealed class GmailInterviewInfo
{
    public DateTimeOffset? ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? InterviewType { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Location { get; set; }
}

public sealed class GmailSuggestedActionInfo
{
    // Reçu en string (ex. "CREATE_INTERVIEW") — voir GmailJobEventMapper.
    public string? ActionType { get; set; }
    public string? SuggestedApplicationStatus { get; set; }
    public DateTimeOffset? DueAt { get; set; }
}

public sealed class GmailEvidenceInfo
{
    public string? ImportantExcerpt { get; set; }
}
