using CareerTrack.Data;
using CareerTrack.DTOs;
using CareerTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers.Api;

// API REST en lecture seule (section 75). Utilise l'authentification par cookie
// existante — un jeton dédié (JWT/API key) serait nécessaire pour un usage machine-à-machine.
[Authorize]
[ApiController]
[Route("api/applications")]
public class ApplicationsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApplicationsApiController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>Liste les candidatures de l'utilisateur courant.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ApplicationSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        List<ApplicationSummaryDto> applications = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Company)
            .Where(a => a.UserId == _currentUser.UserId)
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => new ApplicationSummaryDto(
                a.Id, a.Company.Name, a.JobTitle, a.Status, a.Priority, a.Source, a.AppliedOn, a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(applications);
    }

    /// <summary>Récupère le détail d'une candidature appartenant à l'utilisateur courant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationSummaryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApplicationSummaryDto? application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Company)
            .Where(a => a.Id == id && a.UserId == _currentUser.UserId)
            .Select(a => new ApplicationSummaryDto(
                a.Id, a.Company.Name, a.JobTitle, a.Status, a.Priority, a.Source, a.AppliedOn, a.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        return Ok(application);
    }
}
