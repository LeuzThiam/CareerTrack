namespace CareerTrack.Models.Enums;

// Classification produite par le LLM côté n8n (section 6 de la spec d'intégration).
public enum ImportedEmailType
{
    JobAlert,
    ApplicationConfirmation,
    RecruiterMessage,
    InformationRequest,
    InterviewInvitation,
    InterviewRescheduled,
    AssessmentRequest,
    DocumentRequest,
    ApplicationRejection,
    JobOffer,
    ApplicationWithdrawal,
    OtherJobMessage,
    NotJobRelated
}
