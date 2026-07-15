using System.Security.Claims;

namespace CareerTrack.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            // Identity stocke toujours l'Id utilisateur en string dans le claim NameIdentifier,
            // même si ApplicationUser utilise un Guid comme clé primaire — d'où le parsing.
            string? value = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out Guid userId)
                ? userId
                : throw new InvalidOperationException("L'utilisateur courant est introuvable.");
        }
    }
}
