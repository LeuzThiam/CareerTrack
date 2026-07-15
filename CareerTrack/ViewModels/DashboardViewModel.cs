using CareerTrack.Models;
using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

public class DashboardViewModel
{
    // Cartes KPI
    public int TotalApplications { get; set; }
    public int ActiveApplications { get; set; }
    public int FollowUpsDueTodayOrOverdue { get; set; }
    public int UpcomingInterviewsCount { get; set; }
    public int ResponsesReceived { get; set; }
    public int OffersReceived { get; set; }

    // Indicateurs (section 29)
    public decimal ResponseRate { get; set; }
    public decimal InterviewRate { get; set; }
    public decimal OfferRate { get; set; }
    public decimal RejectionRate { get; set; }

    // Graphiques
    public Dictionary<ApplicationStatus, int> ApplicationsByStatus { get; set; } = [];
    public Dictionary<JobSource, int> ApplicationsBySource { get; set; } = [];
    public Dictionary<string, int> ApplicationsByWeek { get; set; } = [];

    // Sections opérationnelles
    public List<Interview> UpcomingInterviews { get; set; } = [];
    public List<FollowUp> FollowUpsToday { get; set; } = [];
    public List<FollowUp> FollowUpsOverdue { get; set; } = [];
    public List<JobApplication> StaleApplications { get; set; } = [];
    public List<JobOffer> OffersClosingSoon { get; set; } = [];
}
