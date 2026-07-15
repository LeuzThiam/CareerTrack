using CareerTrack.Data;
using CareerTrack.Exceptions;
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
public class JobApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<JobApplicationsController> _logger;

    public JobApplicationsController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<JobApplicationsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] JobApplicationSearchViewModel criteria, CancellationToken cancellationToken)
    {
        criteria.PageNumber = Math.Max(1, criteria.PageNumber);
        criteria.PageSize = criteria.PageSize is > 0 and <= 100 ? criteria.PageSize : 10;

        IQueryable<JobApplication> query = _context.Applications
            .AsNoTracking()
            .Where(application => application.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            string term = criteria.Search.Trim();
            query = query.Where(application =>
                application.JobTitle.Contains(term) || application.Company.Name.Contains(term));
        }

        if (criteria.CompanyId.HasValue)
        {
            query = query.Where(application => application.CompanyId == criteria.CompanyId.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(application => application.Status == criteria.Status.Value);
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(application => application.Priority == criteria.Priority.Value);
        }

        if (criteria.Source.HasValue)
        {
            query = query.Where(application => application.Source == criteria.Source.Value);
        }

        if (criteria.FromDate.HasValue)
        {
            query = query.Where(application => application.AppliedOn >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(application => application.AppliedOn <= criteria.ToDate.Value);
        }

        query = criteria.SortBy switch
        {
            ApplicationSortBy.AppliedOnDesc => query.OrderByDescending(a => a.AppliedOn),
            ApplicationSortBy.AppliedOnAsc => query.OrderBy(a => a.AppliedOn),
            ApplicationSortBy.JobTitleAsc => query.OrderBy(a => a.JobTitle),
            ApplicationSortBy.CompanyNameAsc => query.OrderBy(a => a.Company.Name),
            ApplicationSortBy.PriorityDesc => query.OrderByDescending(a => a.Priority),
            _ => query.OrderByDescending(a => a.UpdatedAt)
        };

        int totalCount = await query.CountAsync(cancellationToken);

        List<JobApplicationListItemViewModel> items = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(a => new JobApplicationListItemViewModel
            {
                Id = a.Id,
                JobTitle = a.JobTitle,
                CompanyName = a.Company.Name,
                Status = a.Status,
                Priority = a.Priority,
                AppliedOn = a.AppliedOn
            })
            .ToListAsync(cancellationToken);

        criteria.Results = new PagedResult<JobApplicationListItemViewModel>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize
        };

        criteria.AvailableCompanies = await _context.Companies
            .AsNoTracking()
            .Where(c => c.UserId == _currentUser.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyOptionViewModel { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);

        return View(criteria);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Company)
            .Include(a => a.JobOffer)
            .Include(a => a.StatusHistory)
            .SingleOrDefaultAsync(a => a.Id == id && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var changeStatusModel = new ChangeStatusViewModel
        {
            ApplicationId = application.Id,
            AvailableStatuses = Enum.GetValues<ApplicationStatus>()
                .Where(s => ApplicationStatusTransitions.IsAllowed(application.Status, s))
                .ToList()
        };

        List<FollowUp> followUps = await _context.FollowUps
            .AsNoTracking()
            .Where(f => f.ApplicationId == application.Id)
            .OrderBy(f => f.ScheduledAt)
            .ToListAsync(cancellationToken);

        List<Interview> interviews = await _context.Interviews
            .AsNoTracking()
            .Where(i => i.ApplicationId == application.Id)
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);

        List<ApplicationDocument> documents = await _context.Documents
            .AsNoTracking()
            .Where(d => d.ApplicationId == application.Id)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

        ViewData["ChangeStatus"] = changeStatusModel;
        ViewData["FollowUps"] = followUps;
        ViewData["NewFollowUp"] = new FollowUpFormViewModel { ApplicationId = application.Id };
        ViewData["Interviews"] = interviews;
        ViewData["NewInterview"] = new InterviewFormViewModel { ApplicationId = application.Id };
        ViewData["Documents"] = documents;

        return View(application);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new JobApplicationFormViewModel
        {
            AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken),
            AvailableJobOffers = await GetAvailableJobOffersAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobApplicationFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateCrossReferencesAsync(model, cancellationToken);
        ValidateBusinessRules(model, model.Status);

        if (!ModelState.IsValid)
        {
            model.AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken);
            model.AvailableJobOffers = await GetAvailableJobOffersAsync(cancellationToken);
            return View(model);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            CompanyId = model.CompanyId,
            JobOfferId = model.JobOfferId,
            JobTitle = model.JobTitle.Trim(),
            AppliedOn = model.AppliedOn,
            Priority = model.Priority,
            Source = model.Source,
            ExpectedSalary = model.ExpectedSalary,
            CurrencyCode = model.CurrencyCode?.Trim().ToUpperInvariant(),
            Notes = model.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        application.InitializeStatus(model.Status, now);

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Candidature {ApplicationId} créée par l'utilisateur {UserId}",
            application.Id, _currentUser.UserId);

        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == id && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var model = new JobApplicationFormViewModel
        {
            Id = application.Id,
            RowVersion = application.RowVersion,
            CompanyId = application.CompanyId,
            JobOfferId = application.JobOfferId,
            JobTitle = application.JobTitle,
            AppliedOn = application.AppliedOn,
            Priority = application.Priority,
            Source = application.Source,
            ExpectedSalary = application.ExpectedSalary,
            CurrencyCode = application.CurrencyCode,
            Notes = application.Notes,
            AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken),
            AvailableJobOffers = await GetAvailableJobOffersAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, JobApplicationFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == id && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        await ValidateCrossReferencesAsync(model, cancellationToken);
        // Le formulaire d'édition ne modifie pas le statut : on valide les règles
        // métier par rapport au statut réel de la candidature, pas au défaut du ViewModel.
        ValidateBusinessRules(model, application.Status);

        if (!ModelState.IsValid)
        {
            model.AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken);
            model.AvailableJobOffers = await GetAvailableJobOffersAsync(cancellationToken);
            return View(model);
        }

        application.CompanyId = model.CompanyId;
        application.JobOfferId = model.JobOfferId;
        application.JobTitle = model.JobTitle.Trim();
        application.AppliedOn = model.AppliedOn;
        application.Priority = model.Priority;
        application.Source = model.Source;
        application.ExpectedSalary = model.ExpectedSalary;
        application.CurrencyCode = model.CurrencyCode?.Trim().ToUpperInvariant();
        application.Notes = model.Notes?.Trim();
        application.UpdatedAt = DateTimeOffset.UtcNow;

        // La valeur soumise par le formulaire représente l'état connu de l'utilisateur ;
        // si un autre onglet a modifié la candidature entre-temps, le RowVersion en base
        // ne correspondra plus et SaveChangesAsync lèvera DbUpdateConcurrencyException.
        _context.Entry(application).Property(a => a.RowVersion).OriginalValue = model.RowVersion;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty,
                "Cette candidature a été modifiée depuis son ouverture. Rechargez la page avant de continuer.");
            model.AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken);
            model.AvailableJobOffers = await GetAvailableJobOffersAsync(cancellationToken);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeStatusViewModel model, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == model.ApplicationId && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        // Règle 6 : le statut Submitted exige une date de candidature.
        if (model.NewStatus == ApplicationStatus.Submitted && application.AppliedOn is null)
        {
            TempData["ChangeStatusError"] = "Impossible de passer à \"Envoyée\" sans date d'envoi. Modifie d'abord la candidature.";
            return RedirectToAction(nameof(Details), new { id = application.Id });
        }

        ApplicationStatusHistory historyEntry;
        try
        {
            historyEntry = application.ChangeStatus(model.NewStatus, model.Comment?.Trim(), DateTimeOffset.UtcNow);
        }
        catch (InvalidStatusTransitionException ex)
        {
            TempData["ChangeStatusError"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = application.Id });
        }

        _context.ApplicationStatusHistories.Add(historyEntry);

        // Règle 13 : une candidature terminale ne doit plus générer de rappel.
        if (ApplicationStatusTransitions.IsTerminal(model.NewStatus))
        {
            List<FollowUp> pendingFollowUps = await _context.FollowUps
                .Where(f => f.ApplicationId == application.Id && !f.IsCompleted)
                .ToListAsync(cancellationToken);

            foreach (FollowUp followUp in pendingFollowUps)
            {
                followUp.IsCompleted = true;
                followUp.CompletedAt = DateTimeOffset.UtcNow;
                followUp.Result = "Annulée automatiquement (candidature terminale)";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Candidature {ApplicationId} : statut changé vers {NewStatus} par {UserId}",
            application.Id, model.NewStatus, _currentUser.UserId);

        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        JobApplication? application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == id && a.UserId == _currentUser.UserId, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        application.ArchivedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Candidature {ApplicationId} archivée par {UserId}",
            application.Id, _currentUser.UserId);

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCrossReferencesAsync(JobApplicationFormViewModel model, CancellationToken cancellationToken)
    {
        bool companyBelongsToUser = await _context.Companies
            .AnyAsync(c => c.Id == model.CompanyId && c.UserId == _currentUser.UserId, cancellationToken);

        if (!companyBelongsToUser)
        {
            ModelState.AddModelError(nameof(model.CompanyId), "Entreprise invalide.");
        }

        if (model.JobOfferId.HasValue)
        {
            bool offerBelongsToUser = await _context.JobOffers
                .AnyAsync(o => o.Id == model.JobOfferId && o.UserId == _currentUser.UserId, cancellationToken);

            if (!offerBelongsToUser)
            {
                ModelState.AddModelError(nameof(model.JobOfferId), "Offre invalide.");
            }
        }
    }

    private void ValidateBusinessRules(JobApplicationFormViewModel model, ApplicationStatus effectiveStatus)
    {
        // Règle 4 : la date de candidature ne doit pas être dans le futur,
        // sauf pour une candidature encore planifiée (ToApply/Preparing).
        bool isPlanned = effectiveStatus is ApplicationStatus.ToApply or ApplicationStatus.Preparing;
        if (model.AppliedOn.HasValue && model.AppliedOn.Value > DateOnly.FromDateTime(DateTime.UtcNow) && !isPlanned)
        {
            ModelState.AddModelError(nameof(model.AppliedOn), "La date de candidature ne peut pas être dans le futur pour ce statut.");
        }

        // Règle 6 : le statut Submitted exige une date de candidature.
        if (effectiveStatus == ApplicationStatus.Submitted && !model.AppliedOn.HasValue)
        {
            ModelState.AddModelError(nameof(model.AppliedOn), "La date d'envoi est requise pour le statut \"Envoyée\".");
        }
    }

    private async Task<List<CompanyOptionViewModel>> GetAvailableCompaniesAsync(CancellationToken cancellationToken)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => c.UserId == _currentUser.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyOptionViewModel { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<JobOfferOptionViewModel>> GetAvailableJobOffersAsync(CancellationToken cancellationToken)
    {
        return await _context.JobOffers
            .AsNoTracking()
            .Where(o => o.UserId == _currentUser.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new JobOfferOptionViewModel { Id = o.Id, Title = o.Title })
            .ToListAsync(cancellationToken);
    }
}
