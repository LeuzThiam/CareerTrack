using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Services;
using CareerTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers;

// Toutes les requêtes filtrent explicitement par UserId (Règle 1 : isolation des données) :
// on ne se fie jamais uniquement à l'Id de l'entité pour retrouver une ressource.
[Authorize]
public class CompaniesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CompaniesController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        IQueryable<Company> query = _context.Companies
            .AsNoTracking()
            .Where(company => company.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(company => company.Name.Contains(term));
        }

        List<Company> companies = await query
            .OrderBy(company => company.Name)
            .ToListAsync(cancellationToken);

        ViewData["Search"] = search;

        return View(companies);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        Company? company = await _context.Companies
            .AsNoTracking()
            .SingleOrDefaultAsync(
                company => company.Id == id && company.UserId == _currentUser.UserId,
                cancellationToken);

        if (company is null)
        {
            return NotFound();
        }

        return View(company);
    }

    public IActionResult Create()
    {
        return View(new CompanyFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var company = new Company
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            Name = model.Name.Trim(),
            Industry = model.Industry?.Trim(),
            WebsiteUrl = model.WebsiteUrl?.Trim(),
            City = model.City?.Trim(),
            Province = model.Province?.Trim(),
            Country = model.Country?.Trim(),
            Notes = model.Notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = company.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        Company? company = await _context.Companies
            .SingleOrDefaultAsync(
                company => company.Id == id && company.UserId == _currentUser.UserId,
                cancellationToken);

        if (company is null)
        {
            return NotFound();
        }

        var model = new CompanyFormViewModel
        {
            Id = company.Id,
            Name = company.Name,
            Industry = company.Industry,
            WebsiteUrl = company.WebsiteUrl,
            City = company.City,
            Province = company.Province,
            Country = company.Country,
            Notes = company.Notes
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CompanyFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Company? company = await _context.Companies
            .SingleOrDefaultAsync(
                company => company.Id == id && company.UserId == _currentUser.UserId,
                cancellationToken);

        if (company is null)
        {
            return NotFound();
        }

        company.Name = model.Name.Trim();
        company.Industry = model.Industry?.Trim();
        company.WebsiteUrl = model.WebsiteUrl?.Trim();
        company.City = model.City?.Trim();
        company.Province = model.Province?.Trim();
        company.Country = model.Country?.Trim();
        company.Notes = model.Notes?.Trim();
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = company.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        Company? company = await _context.Companies
            .SingleOrDefaultAsync(
                company => company.Id == id && company.UserId == _currentUser.UserId,
                cancellationToken);

        if (company is null)
        {
            return NotFound();
        }

        company.ArchivedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}
