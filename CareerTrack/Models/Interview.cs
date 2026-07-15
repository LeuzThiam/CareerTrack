using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

public class Interview
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public InterviewType Type { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
    public DateTimeOffset ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public string? PreparationNotes { get; set; }
    public string? DebriefNotes { get; set; }
    public string? Result { get; set; }

    public JobApplication Application { get; set; } = null!;
}
