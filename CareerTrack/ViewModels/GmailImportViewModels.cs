using CareerTrack.Models;

namespace CareerTrack.ViewModels;

public class GmailImportsOverviewViewModel
{
    public List<ImportedEmail> PendingReview { get; set; } = [];
    public List<ImportedEmail> Processed { get; set; } = [];
    public List<ImportedEmail> Ignored { get; set; } = [];
    public List<ImportedEmail> Duplicates { get; set; } = [];
    public List<ImportedEmail> Errors { get; set; } = [];
}

public class GmailImportDetailsViewModel
{
    public ImportedEmail Import { get; set; } = null!;
    public List<JobApplicationOptionViewModel> AvailableApplications { get; set; } = [];
}

public class JobApplicationOptionViewModel
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
}
