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
public class JobOffersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public JobOffersController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        IQueryable<JobOffer> query = _context.JobOffers
            .AsNoTracking()
            .Include(offer => offer.Company)
            .Where(offer => offer.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(offer =>
                offer.Title.Contains(term) || offer.Company.Name.Contains(term));
        }

        List<JobOffer> offers = await query
            .OrderByDescending(offer => offer.CreatedAt)
            .ToListAsync(cancellationToken);

        ViewData["Search"] = search;

        return View(offers);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        JobOffer? offer = await _context.JobOffers
            .AsNoTracking()
            .Include(o => o.Company)
            .SingleOrDefaultAsync(
                o => o.Id == id && o.UserId == _currentUser.UserId,
                cancellationToken);

        if (offer is null)
        {
            return NotFound();
        }

        return View(offer);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new JobOfferFormViewModel
        {
            AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobOfferFormViewModel model, CancellationToken cancellationToken)
    {
        bool companyBelongsToUser = await _context.Companies
            .AnyAsync(c => c.Id == model.CompanyId && c.UserId == _currentUser.UserId, cancellationToken);

        if (!companyBelongsToUser)
        {
            ModelState.AddModelError(nameof(model.CompanyId), "Entreprise invalide.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken);
            return View(model);
        }

        var offer = new JobOffer
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            CompanyId = model.CompanyId,
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Location = model.Location?.Trim(),
            WorkMode = model.WorkMode,
            EmploymentType = model.EmploymentType,
            Source = model.Source,
            SourceUrl = model.SourceUrl?.Trim(),
            MinimumSalary = model.MinimumSalary,
            MaximumSalary = model.MaximumSalary,
            CurrencyCode = model.CurrencyCode?.Trim().ToUpperInvariant(),
            PublicationDate = model.PublicationDate,
            ClosingDate = model.ClosingDate,
            Status = model.Status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = offer.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        JobOffer? offer = await _context.JobOffers
            .SingleOrDefaultAsync(
                o => o.Id == id && o.UserId == _currentUser.UserId,
                cancellationToken);

        if (offer is null)
        {
            return NotFound();
        }

        var model = new JobOfferFormViewModel
        {
            Id = offer.Id,
            CompanyId = offer.CompanyId,
            Title = offer.Title,
            Description = offer.Description,
            Location = offer.Location,
            WorkMode = offer.WorkMode,
            EmploymentType = offer.EmploymentType,
            Source = offer.Source,
            SourceUrl = offer.SourceUrl,
            MinimumSalary = offer.MinimumSalary,
            MaximumSalary = offer.MaximumSalary,
            CurrencyCode = offer.CurrencyCode,
            PublicationDate = offer.PublicationDate,
            ClosingDate = offer.ClosingDate,
            Status = offer.Status,
            AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, JobOfferFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        bool companyBelongsToUser = await _context.Companies
            .AnyAsync(c => c.Id == model.CompanyId && c.UserId == _currentUser.UserId, cancellationToken);

        if (!companyBelongsToUser)
        {
            ModelState.AddModelError(nameof(model.CompanyId), "Entreprise invalide.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken);
            return View(model);
        }

        JobOffer? offer = await _context.JobOffers
            .SingleOrDefaultAsync(
                o => o.Id == id && o.UserId == _currentUser.UserId,
                cancellationToken);

        if (offer is null)
        {
            return NotFound();
        }

        offer.CompanyId = model.CompanyId;
        offer.Title = model.Title.Trim();
        offer.Description = model.Description?.Trim();
        offer.Location = model.Location?.Trim();
        offer.WorkMode = model.WorkMode;
        offer.EmploymentType = model.EmploymentType;
        offer.Source = model.Source;
        offer.SourceUrl = model.SourceUrl?.Trim();
        offer.MinimumSalary = model.MinimumSalary;
        offer.MaximumSalary = model.MaximumSalary;
        offer.CurrencyCode = model.CurrencyCode?.Trim().ToUpperInvariant();
        offer.PublicationDate = model.PublicationDate;
        offer.ClosingDate = model.ClosingDate;
        offer.Status = model.Status;
        offer.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = offer.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        JobOffer? offer = await _context.JobOffers
            .SingleOrDefaultAsync(
                o => o.Id == id && o.UserId == _currentUser.UserId,
                cancellationToken);

        if (offer is null)
        {
            return NotFound();
        }

        offer.ArchivedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
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
}
