using System.Text.Json;
using CareerTrack.Auth;
using CareerTrack.Data;
using CareerTrack.DTOs;
using CareerTrack.Models;
using CareerTrack.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers.Api;

// Reçoit les événements courriel poussés par n8n (section 8 de la spec d'intégration
// Gmail). CareerTrack ne fait jamais confiance aveuglément à ce que dit le LLM côté
// n8n : tout import reste "à vérifier" en V1, aucune action n'est appliquée automatiquement.
[ApiController]
[Route("api/integrations/gmail")]
[Authorize(AuthenticationSchemes = GmailIntegrationAuthenticationOptions.SchemeName)]
public class GmailIntegrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GmailIntegrationController> _logger;

    public GmailIntegrationController(ApplicationDbContext context, ILogger<GmailIntegrationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("job-events")]
    public async Task<IActionResult> Create(
        [FromBody] GmailJobEventRequest request, CancellationToken cancellationToken)
    {
        Guid userId = GetUserId();

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ExternalMessageId))
        {
            errors.Add("externalMessageId est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.Source))
        {
            errors.Add("source est obligatoire.");
        }

        if (!GmailJobEventMapper.TryMapMessageType(request.Classification.MessageType, out ImportedEmailType messageType))
        {
            errors.Add($"classification.messageType n'est pas reconnu : \"{request.Classification.MessageType}\".");
        }

        if (errors.Count > 0)
        {
            return UnprocessableEntity(GmailJobEventResponse.Rejected(errors));
        }

        // Idempotence (section 11) : n8n peut renvoyer le même message plusieurs fois.
        bool alreadyImported = await _context.ImportedEmails
            .AnyAsync(e => e.UserId == userId && e.Source == request.Source && e.ExternalMessageId == request.ExternalMessageId,
                cancellationToken);

        if (alreadyImported)
        {
            Guid existingId = await _context.ImportedEmails
                .Where(e => e.UserId == userId && e.Source == request.Source && e.ExternalMessageId == request.ExternalMessageId)
                .Select(e => e.Id)
                .SingleAsync(cancellationToken);

            _logger.LogInformation(
                "Import Gmail ignoré (doublon) : {ExternalMessageId} pour {UserId}", request.ExternalMessageId, userId);

            return Ok(GmailJobEventResponse.Duplicate(existingId));
        }

        GmailJobEventMapper.TryMapActionType(request.SuggestedAction?.ActionType, out SuggestedActionType actionType);

        var importedEmail = new ImportedEmail
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Source = request.Source,
            ExternalMessageId = request.ExternalMessageId,
            ExternalThreadId = request.ExternalThreadId,
            SenderName = request.Sender?.Name,
            SenderEmail = request.Sender?.Email,
            Subject = request.Email?.Subject,
            Summary = request.Email?.Summary,
            Language = request.Email?.Language,
            ReceivedAt = request.ReceivedAt,
            ImportedAt = DateTimeOffset.UtcNow,
            MessageType = messageType,
            IsJobRelated = request.Classification.IsJobRelated,
            ConfidenceScore = request.Classification.ConfidenceScore,
            RequiresReview = request.Classification.RequiresReview,
            CompanyNameDetected = request.Job?.CompanyName,
            JobTitleDetected = request.Job?.JobTitle,
            RecruiterNameDetected = request.Recruiter?.Name,
            RecruiterEmailDetected = request.Recruiter?.Email,
            InterviewDateDetected = request.Interview?.ScheduledAt,
            MeetingUrlDetected = request.Interview?.MeetingUrl,
            SuggestedAction = request.SuggestedAction?.ActionType is null ? null : actionType,
            SuggestedApplicationStatus = request.SuggestedAction?.SuggestedApplicationStatus,
            ImportantExcerpt = request.Evidence?.ImportantExcerpt,
            // V1 : validation humaine systématique, quel que soit le score de confiance
            // (section 14 de la spec — pas d'automatisation en première version).
            ProcessingStatus = EmailImportStatus.PendingReview,
            RawPayloadJson = JsonSerializer.Serialize(request)
        };

        _context.ImportedEmails.Add(importedEmail);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Import Gmail {ImportId} reçu pour {UserId} : {MessageType} ({Company} / {JobTitle})",
            importedEmail.Id, userId, messageType, importedEmail.CompanyNameDetected, importedEmail.JobTitleDetected);

        // 201 sans Location : il n'existe pas d'endpoint GET dédié à un import précis
        // pour l'instant (consultation via la page "Imports Gmail", pas via l'API).
        return StatusCode(StatusCodes.Status201Created, GmailJobEventResponse.Accepted(importedEmail.Id, requiresReview: true));
    }

    private Guid GetUserId()
    {
        string? value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid userId) ? userId : throw new InvalidOperationException("Utilisateur d'intégration introuvable.");
    }
}
