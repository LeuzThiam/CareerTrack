using CareerTrack.Exceptions;
using CareerTrack.Models;
using CareerTrack.Models.Enums;
using FluentAssertions;
using Xunit;

namespace CareerTrack.UnitTests;

public class JobApplicationTests
{
    private static JobApplication CreateApplication(ApplicationStatus initialStatus = ApplicationStatus.ToApply)
    {
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobTitle = "Analyste de données"
        };

        application.InitializeStatus(initialStatus, DateTimeOffset.UtcNow);
        return application;
    }

    [Fact]
    public void ChangeStatus_ShouldThrow_WhenTransitionIsInvalid()
    {
        JobApplication application = CreateApplication(ApplicationStatus.ToApply);

        Action action = () =>
            application.ChangeStatus(ApplicationStatus.OfferAccepted, null, DateTimeOffset.UtcNow);

        action.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void ChangeStatus_ShouldSucceed_WhenTransitionIsValid()
    {
        JobApplication application = CreateApplication(ApplicationStatus.ToApply);

        ApplicationStatusHistory history = application.ChangeStatus(
            ApplicationStatus.Submitted, "Envoyée par courriel", DateTimeOffset.UtcNow);

        application.Status.Should().Be(ApplicationStatus.Submitted);
        history.PreviousStatus.Should().Be(ApplicationStatus.ToApply);
        history.NewStatus.Should().Be(ApplicationStatus.Submitted);
        history.Comment.Should().Be("Envoyée par courriel");
    }

    [Fact]
    public void ChangeStatus_ShouldNotChangeStatus_WhenTransitionIsInvalid()
    {
        JobApplication application = CreateApplication(ApplicationStatus.ToApply);

        Action action = () =>
            application.ChangeStatus(ApplicationStatus.OfferAccepted, null, DateTimeOffset.UtcNow);

        action.Should().Throw<InvalidStatusTransitionException>();
        application.Status.Should().Be(ApplicationStatus.ToApply);
    }

    [Fact]
    public void InitializeStatus_ShouldSetStatusAndCreateHistoryEntryWithNoPreviousStatus()
    {
        var application = new JobApplication { Id = Guid.NewGuid(), JobTitle = "Analyste" };
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        application.InitializeStatus(ApplicationStatus.ToApply, createdAt);

        application.Status.Should().Be(ApplicationStatus.ToApply);
        application.StatusHistory.Should().ContainSingle();
        application.StatusHistory.Single().PreviousStatus.Should().BeNull();
        application.StatusHistory.Single().NewStatus.Should().Be(ApplicationStatus.ToApply);
    }

    [Fact]
    public void ChangeStatus_FromTerminalStatus_ShouldOnlyAllowArchiving()
    {
        JobApplication application = CreateApplication(ApplicationStatus.Rejected);

        Action toSubmitted = () =>
            application.ChangeStatus(ApplicationStatus.Submitted, null, DateTimeOffset.UtcNow);
        Action toArchived = () =>
            application.ChangeStatus(ApplicationStatus.Archived, null, DateTimeOffset.UtcNow);

        toSubmitted.Should().Throw<InvalidStatusTransitionException>();
        toArchived.Should().NotThrow();
    }
}
