using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

public class ApplicationStatusHistory
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public ApplicationStatus? PreviousStatus { get; set; }
    public ApplicationStatus NewStatus { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset ChangedAt { get; set; }

    public JobApplication Application { get; set; } = null!;
}
