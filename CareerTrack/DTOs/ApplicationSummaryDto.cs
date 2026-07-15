using CareerTrack.Models.Enums;

namespace CareerTrack.DTOs;

public sealed record ApplicationSummaryDto(
    Guid Id,
    string CompanyName,
    string JobTitle,
    ApplicationStatus Status,
    ApplicationPriority Priority,
    JobSource Source,
    DateOnly? AppliedOn,
    DateTimeOffset UpdatedAt
);
