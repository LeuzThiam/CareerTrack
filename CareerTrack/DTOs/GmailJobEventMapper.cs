using CareerTrack.Models.Enums;

namespace CareerTrack.DTOs;

// Mapping explicite entre les codes JSON envoyés par n8n (ex. "INTERVIEW_INVITATION")
// et les enums C#. Volontairement pas de conversion automatique par nom/attribut :
// un code inconnu doit être détecté ici, pas absorbé silencieusement (section 6 de la spec).
public static class GmailJobEventMapper
{
    private static readonly Dictionary<string, ImportedEmailType> MessageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["JOB_ALERT"] = ImportedEmailType.JobAlert,
        ["APPLICATION_CONFIRMATION"] = ImportedEmailType.ApplicationConfirmation,
        ["RECRUITER_MESSAGE"] = ImportedEmailType.RecruiterMessage,
        ["INFORMATION_REQUEST"] = ImportedEmailType.InformationRequest,
        ["INTERVIEW_INVITATION"] = ImportedEmailType.InterviewInvitation,
        ["INTERVIEW_RESCHEDULED"] = ImportedEmailType.InterviewRescheduled,
        ["ASSESSMENT_REQUEST"] = ImportedEmailType.AssessmentRequest,
        ["DOCUMENT_REQUEST"] = ImportedEmailType.DocumentRequest,
        ["APPLICATION_REJECTION"] = ImportedEmailType.ApplicationRejection,
        ["JOB_OFFER"] = ImportedEmailType.JobOffer,
        ["APPLICATION_WITHDRAWAL"] = ImportedEmailType.ApplicationWithdrawal,
        ["OTHER_JOB_MESSAGE"] = ImportedEmailType.OtherJobMessage,
        ["NOT_JOB_RELATED"] = ImportedEmailType.NotJobRelated
    };

    private static readonly Dictionary<string, SuggestedActionType> ActionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CREATE_APPLICATION"] = SuggestedActionType.CreateApplication,
        ["UPDATE_APPLICATION_STATUS"] = SuggestedActionType.UpdateApplicationStatus,
        ["CREATE_INTERVIEW"] = SuggestedActionType.CreateInterview,
        ["RESCHEDULE_INTERVIEW"] = SuggestedActionType.RescheduleInterview,
        ["CREATE_FOLLOW_UP"] = SuggestedActionType.CreateFollowUp,
        ["CANCEL_FOLLOW_UPS"] = SuggestedActionType.CancelFollowUps,
        ["CREATE_CONTACT"] = SuggestedActionType.CreateContact,
        ["ATTACH_DOCUMENT"] = SuggestedActionType.AttachDocument,
        ["MARK_AS_REJECTED"] = SuggestedActionType.MarkAsRejected,
        ["MARK_AS_OFFER_RECEIVED"] = SuggestedActionType.MarkAsOfferReceived,
        ["REVIEW_ONLY"] = SuggestedActionType.ReviewOnly,
        ["IGNORE"] = SuggestedActionType.Ignore
    };

    public static bool TryMapMessageType(string? code, out ImportedEmailType result)
    {
        if (code is not null && MessageTypes.TryGetValue(code, out ImportedEmailType mapped))
        {
            result = mapped;
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryMapActionType(string? code, out SuggestedActionType result)
    {
        if (code is not null && ActionTypes.TryGetValue(code, out SuggestedActionType mapped))
        {
            result = mapped;
            return true;
        }

        result = default;
        return false;
    }
}
