using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

public class FollowUp
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public FollowUpType Type { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string? Result { get; set; }
    public string? Notes { get; set; }

    public JobApplication Application { get; set; } = null!;
}
