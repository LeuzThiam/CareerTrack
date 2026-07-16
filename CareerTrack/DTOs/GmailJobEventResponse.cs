namespace CareerTrack.DTOs;

// Reflète les formats de réponse de la section 16 de la spec d'intégration.
public sealed class GmailJobEventResponse
{
    public string Status { get; init; } = string.Empty;
    public Guid? ImportId { get; init; }
    public bool? RequiresReview { get; init; }
    public string? Reason { get; init; }
    public List<string>? Errors { get; init; }

    public static GmailJobEventResponse Accepted(Guid importId, bool requiresReview) =>
        new() { Status = "accepted", ImportId = importId, RequiresReview = requiresReview };

    public static GmailJobEventResponse Duplicate(Guid importId) =>
        new() { Status = "duplicate", ImportId = importId };

    public static GmailJobEventResponse Rejected(List<string> errors) =>
        new() { Status = "rejected", Errors = errors };
}
