namespace CareerTrack.Models.Enums;

// Action proposée par n8n (section 13) : une suggestion à revalider, jamais une commande
// exécutée automatiquement en V1 — voir ImportedEmail.SuggestedAction et Règle de validation humaine.
public enum SuggestedActionType
{
    CreateApplication,
    UpdateApplicationStatus,
    CreateInterview,
    RescheduleInterview,
    CreateFollowUp,
    CancelFollowUps,
    CreateContact,
    AttachDocument,
    MarkAsRejected,
    MarkAsOfferReceived,
    ReviewOnly,
    Ignore
}
