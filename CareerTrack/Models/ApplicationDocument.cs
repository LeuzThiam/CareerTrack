using CareerTrack.Models.Enums;

namespace CareerTrack.Models;

public class ApplicationDocument
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    public JobApplication Application { get; set; } = null!;
}
