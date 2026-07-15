using System.Globalization;
using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Models.Enums;
using CareerTrack.Services;
using CareerTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers;

// Statuts considérés comme "une réponse a été reçue" (section 78 du cahier des charges).
[Authorize]
public class DashboardController : Controller
{
    private static readonly ApplicationStatus[] ResponseStatuses =
    [
        ApplicationStatus.PhoneInterview, ApplicationStatus.HrInterview,
        ApplicationStatus.TechnicalInterview, ApplicationStatus.FinalInterview,
        ApplicationStatus.TestRequired, ApplicationStatus.OfferReceived,
        ApplicationStatus.OfferAccepted, ApplicationStatus.OfferDeclined,
        ApplicationStatus.Rejected
    ];

    private static readonly ApplicationStatus[] InterviewStatuses =
    [
        ApplicationStatus.PhoneInterview, ApplicationStatus.HrInterview,
        ApplicationStatus.TechnicalInterview, ApplicationStatus.FinalInterview
    ];

    private static readonly ApplicationStatus[] OfferStatuses =
    [
        ApplicationStatus.OfferReceived, ApplicationStatus.OfferAccepted, ApplicationStatus.OfferDeclined
    ];

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Guid userId = _currentUser.UserId;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        List<JobApplication> applications = await _context.Applications
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        int submittedCount = applications.Count(a => a.AppliedOn != null);
        int responseCount = applications.Count(a => ResponseStatuses.Contains(a.Status));
        int interviewCount = applications.Count(a => InterviewStatuses.Contains(a.Status) || OfferStatuses.Contains(a.Status));
        int offerCount = applications.Count(a => OfferStatuses.Contains(a.Status));
        int rejectedCount = applications.Count(a => a.Status == ApplicationStatus.Rejected);

        var model = new DashboardViewModel
        {
            TotalApplications = applications.Count,
            ActiveApplications = applications.Count(a => !ApplicationStatusTransitions.IsTerminal(a.Status)),
            ResponsesReceived = responseCount,
            OffersReceived = offerCount,

            ResponseRate = submittedCount == 0 ? 0 : decimal.Round(responseCount * 100m / submittedCount, 1),
            InterviewRate = submittedCount == 0 ? 0 : decimal.Round(interviewCount * 100m / submittedCount, 1),
            OfferRate = submittedCount == 0 ? 0 : decimal.Round(offerCount * 100m / submittedCount, 1),
            RejectionRate = submittedCount == 0 ? 0 : decimal.Round(rejectedCount * 100m / submittedCount, 1),

            ApplicationsByStatus = applications
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count()),

            ApplicationsBySource = applications
                .GroupBy(a => a.Source)
                .ToDictionary(g => g.Key, g => g.Count()),

            ApplicationsByWeek = applications
                .Where(a => a.AppliedOn != null)
                .GroupBy(a => ISOWeek.GetWeekOfYear(a.AppliedOn!.Value.ToDateTime(TimeOnly.MinValue)))
                .OrderBy(g => g.Key)
                .ToDictionary(g => $"Semaine {g.Key}", g => g.Count())
        };

        model.UpcomingInterviews = await _context.Interviews
            .AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Company)
            .Where(i => i.UserId == userId && i.Status == InterviewStatus.Scheduled && i.ScheduledAt >= now)
            .OrderBy(i => i.ScheduledAt)
            .Take(5)
            .ToListAsync(cancellationToken);
        model.UpcomingInterviewsCount = await _context.Interviews
            .AsNoTracking()
            .CountAsync(i => i.UserId == userId && i.Status == InterviewStatus.Scheduled && i.ScheduledAt >= now, cancellationToken);

        model.FollowUpsToday = await _context.FollowUps
            .AsNoTracking()
            .Include(f => f.Application).ThenInclude(a => a.Company)
            .Where(f => f.UserId == userId && !f.IsCompleted && f.ScheduledAt >= now.Date && f.ScheduledAt < now.Date.AddDays(1))
            .OrderBy(f => f.ScheduledAt)
            .ToListAsync(cancellationToken);

        model.FollowUpsOverdue = await _context.FollowUps
            .AsNoTracking()
            .Include(f => f.Application).ThenInclude(a => a.Company)
            .Where(f => f.UserId == userId && !f.IsCompleted && f.ScheduledAt < now.Date)
            .OrderBy(f => f.ScheduledAt)
            .ToListAsync(cancellationToken);

        model.FollowUpsDueTodayOrOverdue = model.FollowUpsToday.Count + model.FollowUpsOverdue.Count;

        DateTimeOffset staleThreshold = now.AddDays(-14);
        model.StaleApplications = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Company)
            .Where(a => a.UserId == userId && a.UpdatedAt < staleThreshold)
            .Where(a => !ApplicationStatusTransitions.TerminalStatuses.Contains(a.Status))
            .OrderBy(a => a.UpdatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        DateOnly closingSoonThreshold = DateOnly.FromDateTime(now.AddDays(14).UtcDateTime);
        model.OffersClosingSoon = await _context.JobOffers
            .AsNoTracking()
            .Include(o => o.Company)
            .Where(o => o.UserId == userId && o.ClosingDate != null &&
                        o.ClosingDate >= DateOnly.FromDateTime(now.UtcDateTime) && o.ClosingDate <= closingSoonThreshold)
            .OrderBy(o => o.ClosingDate)
            .ToListAsync(cancellationToken);

        return View(model);
    }
}
