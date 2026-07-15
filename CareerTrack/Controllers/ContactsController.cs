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
public class ContactsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ContactsController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        IQueryable<Contact> query = _context.Contacts
            .AsNoTracking()
            .Include(c => c.Company)
            .Where(c => c.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(c =>
                c.FirstName.Contains(term) || c.LastName.Contains(term) || c.Company.Name.Contains(term));
        }

        List<Contact> contacts = await query
            .OrderBy(c => c.LastName)
            .ToListAsync(cancellationToken);

        ViewData["Search"] = search;

        return View(contacts);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        Contact? contact = await _context.Contacts
            .AsNoTracking()
            .Include(c => c.Company)
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        return View(contact);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ContactFormViewModel
        {
            AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContactFormViewModel model, CancellationToken cancellationToken)
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

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            CompanyId = model.CompanyId,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            JobTitle = model.JobTitle?.Trim(),
            Email = model.Email?.Trim(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            LinkedInUrl = model.LinkedInUrl?.Trim(),
            Notes = model.Notes?.Trim()
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = contact.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        Contact? contact = await _context.Contacts
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        var model = new ContactFormViewModel
        {
            Id = contact.Id,
            CompanyId = contact.CompanyId,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            JobTitle = contact.JobTitle,
            Email = contact.Email,
            PhoneNumber = contact.PhoneNumber,
            LinkedInUrl = contact.LinkedInUrl,
            Notes = contact.Notes,
            AvailableCompanies = await GetAvailableCompaniesAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ContactFormViewModel model, CancellationToken cancellationToken)
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

        Contact? contact = await _context.Contacts
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        contact.CompanyId = model.CompanyId;
        contact.FirstName = model.FirstName.Trim();
        contact.LastName = model.LastName.Trim();
        contact.JobTitle = model.JobTitle?.Trim();
        contact.Email = model.Email?.Trim();
        contact.PhoneNumber = model.PhoneNumber?.Trim();
        contact.LinkedInUrl = model.LinkedInUrl?.Trim();
        contact.Notes = model.Notes?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = contact.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Contact? contact = await _context.Contacts
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        _context.Contacts.Remove(contact);
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
