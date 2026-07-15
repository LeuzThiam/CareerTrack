using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

// Matrice des transitions valides entre statuts de candidature (section 26-27 du cahier des charges).
// Les statuts terminaux (OfferAccepted, OfferDeclined, Rejected, Withdrawn) ne peuvent
// plus évoluer qu'vers Archived (Règle 8 : une candidature terminale ferme le processus).
public static class ApplicationStatusTransitions
{
    private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> Allowed = new()
    {
        [ApplicationStatus.ToApply] = [ApplicationStatus.Preparing, ApplicationStatus.Submitted, ApplicationStatus.Withdrawn, ApplicationStatus.Archived],
        [ApplicationStatus.Preparing] = [ApplicationStatus.Submitted, ApplicationStatus.Withdrawn, ApplicationStatus.Archived],
        [ApplicationStatus.Submitted] =
        [
            ApplicationStatus.WaitingForResponse, ApplicationStatus.FollowedUp,
            ApplicationStatus.PhoneInterview, ApplicationStatus.HrInterview,
            ApplicationStatus.TechnicalInterview, ApplicationStatus.FinalInterview,
            ApplicationStatus.TestRequired, ApplicationStatus.Rejected,
            ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.WaitingForResponse] =
        [
            ApplicationStatus.FollowedUp, ApplicationStatus.PhoneInterview, ApplicationStatus.HrInterview,
            ApplicationStatus.TechnicalInterview, ApplicationStatus.FinalInterview, ApplicationStatus.TestRequired,
            ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.FollowedUp] =
        [
            ApplicationStatus.PhoneInterview, ApplicationStatus.HrInterview, ApplicationStatus.TechnicalInterview,
            ApplicationStatus.FinalInterview, ApplicationStatus.TestRequired,
            ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.PhoneInterview] =
        [
            ApplicationStatus.HrInterview, ApplicationStatus.TechnicalInterview, ApplicationStatus.FinalInterview,
            ApplicationStatus.TestRequired, ApplicationStatus.OfferReceived,
            ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.HrInterview] =
        [
            ApplicationStatus.TechnicalInterview, ApplicationStatus.FinalInterview, ApplicationStatus.TestRequired,
            ApplicationStatus.OfferReceived, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.TechnicalInterview] =
        [
            ApplicationStatus.FinalInterview, ApplicationStatus.TestRequired, ApplicationStatus.OfferReceived,
            ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.FinalInterview] =
        [
            ApplicationStatus.TestRequired, ApplicationStatus.OfferReceived,
            ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.TestRequired] =
        [
            ApplicationStatus.OfferReceived, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
        ],
        [ApplicationStatus.OfferReceived] = [ApplicationStatus.OfferAccepted, ApplicationStatus.OfferDeclined, ApplicationStatus.Archived],
        [ApplicationStatus.OfferAccepted] = [ApplicationStatus.Archived],
        [ApplicationStatus.OfferDeclined] = [ApplicationStatus.Archived],
        [ApplicationStatus.Rejected] = [ApplicationStatus.Archived],
        [ApplicationStatus.Withdrawn] = [ApplicationStatus.Archived],
        [ApplicationStatus.Archived] = []
    };

    public static readonly ApplicationStatus[] TerminalStatuses =
    [
        ApplicationStatus.OfferAccepted, ApplicationStatus.OfferDeclined,
        ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived
    ];

    public static bool IsAllowed(ApplicationStatus current, ApplicationStatus requested)
    {
        return Allowed.TryGetValue(current, out ApplicationStatus[]? next) && next.Contains(requested);
    }

    public static bool IsTerminal(ApplicationStatus status) => TerminalStatuses.Contains(status);
}
