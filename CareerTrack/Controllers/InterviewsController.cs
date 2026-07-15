using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Services;
using CareerTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers;

// Toutes les requêtes filtrent explicitement par UserId (Règle 1 : isolation des données).
[Authorize]
public class InterviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public InterviewsController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InterviewFormViewModel model, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == model.ApplicationId && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        // Règle 11 : une entrevue planifiée doit avoir une date future.
        if (model.ScheduledAt <= DateTimeOffset.UtcNow)
        {
            TempData["InterviewError"] = "La date d'une entrevue planifiée doit être dans le futur.";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = application.Id });
        }

        var interview = new Interview
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            ApplicationId = application.Id,
            Type = model.Type,
            Status = Models.Enums.InterviewStatus.Scheduled,
            ScheduledAt = model.ScheduledAt,
            DurationMinutes = model.DurationMinutes,
            Location = model.Location?.Trim(),
            MeetingUrl = model.MeetingUrl?.Trim(),
            PreparationNotes = model.PreparationNotes?.Trim()
        };

        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = application.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(Guid interviewId, Guid applicationId, DateTimeOffset newScheduledAt, CancellationToken cancellationToken)
    {
        Interview? interview = await _context.Interviews
            .SingleOrDefaultAsync(i => i.Id == interviewId && i.UserId == _currentUser.UserId, cancellationToken);

        if (interview is null)
        {
            return NotFound();
        }

        if (newScheduledAt <= DateTimeOffset.UtcNow)
        {
            TempData["InterviewError"] = "La nouvelle date doit être dans le futur.";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
        }

        interview.ScheduledAt = newScheduledAt;
        interview.Status = Models.Enums.InterviewStatus.Rescheduled;
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(CompleteInterviewViewModel model, CancellationToken cancellationToken)
    {
        Interview? interview = await _context.Interviews
            .SingleOrDefaultAsync(i => i.Id == model.InterviewId && i.UserId == _currentUser.UserId, cancellationToken);

        if (interview is null)
        {
            return NotFound();
        }

        interview.Status = model.Status;
        interview.DebriefNotes = model.DebriefNotes?.Trim();
        interview.Result = model.Result?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = model.ApplicationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid interviewId, Guid applicationId, CancellationToken cancellationToken)
    {
        Interview? interview = await _context.Interviews
            .SingleOrDefaultAsync(i => i.Id == interviewId && i.UserId == _currentUser.UserId, cancellationToken);

        if (interview is null)
        {
            return NotFound();
        }

        interview.Status = Models.Enums.InterviewStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
    }
}
