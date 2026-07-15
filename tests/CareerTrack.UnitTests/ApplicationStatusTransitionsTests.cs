using CareerTrack.Models;
using CareerTrack.Models.Enums;
using FluentAssertions;
using Xunit;

namespace CareerTrack.UnitTests;

public class ApplicationStatusTransitionsTests
{
    [Fact]
    public void IsAllowed_ToApplyToOfferAccepted_ShouldBeFalse()
    {
        // Le saut suspect explicitement cité en exemple dans le cahier des charges (section 26).
        ApplicationStatusTransitions.IsAllowed(ApplicationStatus.ToApply, ApplicationStatus.OfferAccepted)
            .Should().BeFalse();
    }

    [Theory]
    [InlineData(ApplicationStatus.ToApply, ApplicationStatus.Preparing)]
    [InlineData(ApplicationStatus.ToApply, ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.PhoneInterview)]
    [InlineData(ApplicationStatus.PhoneInterview, ApplicationStatus.OfferReceived)]
    [InlineData(ApplicationStatus.OfferReceived, ApplicationStatus.OfferAccepted)]
    public void IsAllowed_ForExpectedForwardTransitions_ShouldBeTrue(ApplicationStatus from, ApplicationStatus to)
    {
        ApplicationStatusTransitions.IsAllowed(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(ApplicationStatus.OfferAccepted)]
    [InlineData(ApplicationStatus.OfferDeclined)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Withdrawn)]
    [InlineData(ApplicationStatus.Archived)]
    public void IsTerminal_ForTerminalStatuses_ShouldBeTrue(ApplicationStatus status)
    {
        ApplicationStatusTransitions.IsTerminal(status).Should().BeTrue();
    }

    [Theory]
    [InlineData(ApplicationStatus.ToApply)]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.PhoneInterview)]
    [InlineData(ApplicationStatus.OfferReceived)]
    public void IsTerminal_ForNonTerminalStatuses_ShouldBeFalse(ApplicationStatus status)
    {
        ApplicationStatusTransitions.IsTerminal(status).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_FromArchived_ShouldAlwaysBeFalse()
    {
        // Archived est un cul-de-sac : aucune transition sortante n'est permise.
        foreach (ApplicationStatus target in Enum.GetValues<ApplicationStatus>())
        {
            ApplicationStatusTransitions.IsAllowed(ApplicationStatus.Archived, target).Should().BeFalse();
        }
    }

    [Theory]
    [InlineData(ApplicationStatus.OfferAccepted)]
    [InlineData(ApplicationStatus.OfferDeclined)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public void IsAllowed_FromTerminalNonArchivedStatus_OnlyAllowsArchiving(ApplicationStatus from)
    {
        foreach (ApplicationStatus target in Enum.GetValues<ApplicationStatus>())
        {
            bool allowed = ApplicationStatusTransitions.IsAllowed(from, target);
            if (target == ApplicationStatus.Archived)
            {
                allowed.Should().BeTrue();
            }
            else
            {
                allowed.Should().BeFalse();
            }
        }
    }
}
