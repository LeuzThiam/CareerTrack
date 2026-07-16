using Microsoft.AspNetCore.Authentication;

namespace CareerTrack.Auth;

public class GmailIntegrationAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "GmailIntegration";

    // Secret partagé, conservé côté n8n (credential Bearer) et côté CareerTrack (config/secrets),
    // jamais dans Git — voir section 8 de la spec d'intégration.
    public string Secret { get; set; } = string.Empty;

    // Un seul compte utilise cette intégration pour l'instant (le Gmail connecté à n8n
    // correspond à un seul utilisateur CareerTrack) : pas de gestion multi-clés en V1.
    public Guid UserId { get; set; }
}
