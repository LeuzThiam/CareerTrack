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
public class GmailImportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GmailImportsController> _logger;

    public GmailImportsController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<GmailImportsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<ImportedEmail> imports = await _context.ImportedEmails
            .AsNoTracking()
            .Where(e => e.UserId == _currentUser.UserId)
            .OrderByDescending(e => e.ImportedAt)
            .ToListAsync(cancellationToken);

        var model = new GmailImportsOverviewViewModel
        {
            PendingReview = imports.Where(e => e.ProcessingStatus == EmailImportStatus.PendingReview || e.ProcessingStatus == EmailImportStatus.Received).ToList(),
            Processed = imports.Where(e => e.ProcessingStatus == EmailImportStatus.Processed).ToList(),
            Ignored = imports.Where(e => e.ProcessingStatus == EmailImportStatus.Ignored).ToList(),
            Duplicates = imports.Where(e => e.ProcessingStatus == EmailImportStatus.Duplicate).ToList(),
            Errors = imports.Where(e => e.ProcessingStatus is EmailImportStatus.Failed or EmailImportStatus.Rejected).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        ImportedEmail? import = await _context.ImportedEmails
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == _currentUser.UserId, cancellationToken);

        if (import is null)
        {
            return NotFound();
        }

        List<JobApplicationOptionViewModel> availableApplications = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Company)
            .Where(a => a.UserId == _currentUser.UserId)
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => new JobApplicationOptionViewModel { Id = a.Id, Label = a.JobTitle + " — " + a.Company.Name })
            .ToListAsync(cancellationToken);

        var model = new GmailImportDetailsViewModel { Import = import, AvailableApplications = availableApplications };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ignore(Guid id, CancellationToken cancellationToken)
    {
        ImportedEmail? import = await _context.ImportedEmails
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == _currentUser.UserId, cancellationToken);

        if (import is null)
        {
            return NotFound();
        }

        import.ProcessingStatus = EmailImportStatus.Ignored;
        import.ProcessedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Import Gmail {ImportId} ignoré par {UserId}", import.Id, _currentUser.UserId);

        return RedirectToAction(nameof(Index));
    }

    // Associe l'import à une candidature existante choisie par l'utilisateur — jamais
    // sélectionnée automatiquement (section 12 : pas de choix arbitraire par le système).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Associate(Guid id, Guid applicationId, CancellationToken cancellationToken)
    {
        ImportedEmail? import = await _context.ImportedEmails
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == _currentUser.UserId, cancellationToken);

        if (import is null)
        {
            return NotFound();
        }

        bool applicationBelongsToUser = await _context.Applications
            .AnyAsync(a => a.Id == applicationId && a.UserId == _currentUser.UserId, cancellationToken);

        if (!applicationBelongsToUser)
        {
            return BadRequest("Candidature invalide.");
        }

        import.ApplicationId = applicationId;
        import.ProcessingStatus = EmailImportStatus.Processed;
        import.ProcessedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Import Gmail {ImportId} associé à la candidature {ApplicationId} par {UserId}",
            import.Id, applicationId, _currentUser.UserId);

        return RedirectToAction(nameof(Details), new { id = import.Id });
    }
}
