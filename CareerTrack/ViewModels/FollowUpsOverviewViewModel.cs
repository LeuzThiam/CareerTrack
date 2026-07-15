using CareerTrack.Models;

namespace CareerTrack.ViewModels;

public class FollowUpsOverviewViewModel
{
    public List<FollowUp> Overdue { get; set; } = [];
    public List<FollowUp> Today { get; set; } = [];
    public List<FollowUp> Upcoming { get; set; } = [];
    public List<FollowUp> Completed { get; set; } = [];
}
