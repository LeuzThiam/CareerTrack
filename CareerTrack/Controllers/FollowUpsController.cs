using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Models.Enums;
using CareerTrack.Services;
using CareerTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers;

// Toutes les requêtes filtrent explicitement par UserId (Règle 1 : isolation des données).
[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public FollowUpsController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<FollowUp> followUps = await _context.FollowUps
            .AsNoTracking()
            .Include(f => f.Application).ThenInclude(a => a.Company)
            .Where(f => f.UserId == _currentUser.UserId)
            .OrderBy(f => f.ScheduledAt)
            .ToListAsync(cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        var model = new FollowUpsOverviewViewModel
        {
            Overdue = followUps.Where(f => !f.IsCompleted && f.ScheduledAt < now.Date).ToList(),
            Today = followUps.Where(f => !f.IsCompleted && f.ScheduledAt >= now.Date && f.ScheduledAt < now.Date.AddDays(1)).ToList(),
            Upcoming = followUps.Where(f => !f.IsCompleted && f.ScheduledAt >= now.Date.AddDays(1)).ToList(),
            Completed = followUps.Where(f => f.IsCompleted).OrderByDescending(f => f.CompletedAt).Take(20).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUpFormViewModel model, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == model.ApplicationId && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        // Règle 13 : une candidature terminale ne doit pas générer de nouvelle relance.
        if (ApplicationStatusTransitions.IsTerminal(application.Status))
        {
            TempData["FollowUpError"] = "Impossible de planifier une relance : cette candidature est dans un statut terminal.";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = application.Id });
        }

        // Règle 12 : une relance ne doit pas être planifiée avant l'envoi de la candidature.
        if (application.AppliedOn.HasValue &&
            model.ScheduledAt.Date < application.AppliedOn.Value.ToDateTime(TimeOnly.MinValue))
        {
            TempData["FollowUpError"] = "La relance ne peut pas être planifiée avant la date d'envoi de la candidature.";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = application.Id });
        }

        var followUp = new FollowUp
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            ApplicationId = application.Id,
            Type = model.Type,
            ScheduledAt = model.ScheduledAt,
            Notes = model.Notes?.Trim(),
            IsCompleted = false
        };

        _context.FollowUps.Add(followUp);
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = application.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(CompleteFollowUpViewModel model, CancellationToken cancellationToken)
    {
        FollowUp? followUp = await _context.FollowUps
            .SingleOrDefaultAsync(f => f.Id == model.FollowUpId && f.UserId == _currentUser.UserId, cancellationToken);

        if (followUp is null)
        {
            return NotFound();
        }

        followUp.IsCompleted = true;
        followUp.CompletedAt = DateTimeOffset.UtcNow;
        followUp.Result = model.Result?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = model.ApplicationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(Guid followUpId, Guid applicationId, DateTimeOffset newScheduledAt, CancellationToken cancellationToken)
    {
        FollowUp? followUp = await _context.FollowUps
            .SingleOrDefaultAsync(f => f.Id == followUpId && f.UserId == _currentUser.UserId, cancellationToken);

        if (followUp is null)
        {
            return NotFound();
        }

        followUp.ScheduledAt = newScheduledAt;
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid followUpId, Guid applicationId, CancellationToken cancellationToken)
    {
        FollowUp? followUp = await _context.FollowUps
            .SingleOrDefaultAsync(f => f.Id == followUpId && f.UserId == _currentUser.UserId, cancellationToken);

        if (followUp is null)
        {
            return NotFound();
        }

        _context.FollowUps.Remove(followUp);
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
    }
}
