using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

public class JobOffer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public JobSource Source { get; set; }
    public string? SourceUrl { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public string? CurrencyCode { get; set; }
    public DateOnly? PublicationDate { get; set; }
    public DateOnly? ClosingDate { get; set; }
    public JobOfferStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
