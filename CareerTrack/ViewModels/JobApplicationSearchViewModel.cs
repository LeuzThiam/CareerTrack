using CareerTrack.Models.Enums;

namespace CareerTrack.ViewModels;

public enum ApplicationSortBy
{
    UpdatedAtDesc,
    AppliedOnDesc,
    AppliedOnAsc,
    JobTitleAsc,
    CompanyNameAsc,
    PriorityDesc
}

public class JobApplicationSearchViewModel
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public ApplicationStatus? Status { get; set; }
    public ApplicationPriority? Priority { get; set; }
    public JobSource? Source { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public ApplicationSortBy SortBy { get; set; } = ApplicationSortBy.UpdatedAtDesc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public List<CompanyOptionViewModel> AvailableCompanies { get; set; } = [];
    public PagedResult<JobApplicationListItemViewModel> Results { get; set; } = new();
}

public class JobApplicationListItemViewModel
{
    public Guid Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public ApplicationPriority Priority { get; set; }
    public DateOnly? AppliedOn { get; set; }
}
