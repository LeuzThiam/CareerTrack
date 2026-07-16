using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace CareerTrack.Auth;

// Authentifie l'appel poussé par n8n vers l'endpoint d'ingestion Gmail, via le jeton
// Bearer déjà envoyé par le nœud "Send to Web App" (authentication: httpBearerAuth) —
// pas de en-tête personnalisé à configurer côté n8n.
public class GmailIntegrationAuthenticationHandler : AuthenticationHandler<GmailIntegrationAuthenticationOptions>
{
    public GmailIntegrationAuthenticationHandler(
        IOptionsMonitor<GmailIntegrationAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrEmpty(Options.Secret) || Options.UserId == Guid.Empty)
        {
            return Task.FromResult(AuthenticateResult.Fail(
                "Intégration Gmail non configurée (GmailIntegration:Secret / GmailIntegration:UserId manquants)."));
        }

        string? authorizationHeader = Request.Headers[HeaderNames.Authorization].FirstOrDefault();
        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Le schéma d'authentification doit être \"Bearer\"."));
        }

        string providedSecret = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(providedSecret) || !FixedTimeEquals(providedSecret, Options.Secret))
        {
            return Task.FromResult(AuthenticateResult.Fail("Jeton d'intégration invalide."));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Options.UserId.ToString()) };
        var identity = new ClaimsIdentity(claims, GmailIntegrationAuthenticationOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, GmailIntegrationAuthenticationOptions.SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    // Comparaison en temps constant : évite qu'une différence de timing sur la comparaison
    // de chaînes ne laisse fuiter des informations sur le secret attendu.
    private static bool FixedTimeEquals(string a, string b)
    {
        byte[] bytesA = Encoding.UTF8.GetBytes(a);
        byte[] bytesB = Encoding.UTF8.GetBytes(b);

        if (bytesA.Length != bytesB.Length)
        {
            CryptographicOperations.FixedTimeEquals(bytesA, bytesA);
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
