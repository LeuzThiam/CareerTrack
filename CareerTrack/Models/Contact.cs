namespace CareerTrack.Models;

public class Contact
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Notes { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
