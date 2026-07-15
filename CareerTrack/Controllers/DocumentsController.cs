using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Models.Enums;
using CareerTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers;

// Toutes les requêtes filtrent explicitement par UserId (Règle 1 et Règle 14 :
// un document ne peut être téléchargé ou supprimé que par son propriétaire).
[Authorize]
public class DocumentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage,
        IConfiguration configuration,
        ILogger<DocumentsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(Guid applicationId, DocumentType type, IFormFile? file, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == applicationId && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            TempData["DocumentError"] = "Aucun fichier sélectionné.";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
        }

        // Règle 15 : taille maximale des fichiers.
        long maxSize = _configuration.GetValue<long>("FileStorage:MaximumFileSizeBytes");
        if (file.Length > maxSize)
        {
            TempData["DocumentError"] = $"Le fichier dépasse la taille maximale autorisée ({maxSize / 1024 / 1024} Mo).";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
        }

        // Règle 16 : types de fichiers autorisés. L'extension déclarée par le navigateur
        // n'est pas fiable à elle seule, mais on la valide côté serveur avant tout traitement ;
        // seule l'extension déterminée ici sert à nommer le fichier stocké.
        string[] allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? [];
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            TempData["DocumentError"] = $"Type de fichier non autorisé. Extensions acceptées : {string.Join(", ", allowedExtensions)}.";
            return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
        }

        string storedFileName = $"{Guid.NewGuid()}{extension}";
        await _fileStorage.SaveAsync(file, storedFileName, cancellationToken);

        var document = new ApplicationDocument
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            ApplicationId = applicationId,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            Type = type,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        // On journalise le nom original et la taille, jamais le contenu du document (Règle 69).
        _logger.LogInformation(
            "Document {DocumentId} ({FileName}, {SizeInBytes} octets) téléversé par {UserId} pour la candidature {ApplicationId}",
            document.Id, document.OriginalFileName, document.SizeInBytes, _currentUser.UserId, applicationId);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
    }

    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        ApplicationDocument? document = await _context.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == id && d.UserId == _currentUser.UserId, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        Stream stream = _fileStorage.OpenRead(document.StoredFileName);
        return File(stream, document.ContentType, document.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid applicationId, CancellationToken cancellationToken)
    {
        ApplicationDocument? document = await _context.Documents
            .SingleOrDefaultAsync(d => d.Id == id && d.UserId == _currentUser.UserId, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        _fileStorage.Delete(document.StoredFileName);
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Document {DocumentId} supprimé par {UserId}",
            document.Id, _currentUser.UserId);

        return RedirectToAction(nameof(JobApplicationsController.Details), "JobApplications", new { id = applicationId });
    }
}
