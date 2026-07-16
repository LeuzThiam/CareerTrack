using CareerTrack.DTOs;
using CareerTrack.Models.Enums;
using FluentAssertions;
using Xunit;

namespace CareerTrack.UnitTests;

public class GmailJobEventMapperTests
{
    [Theory]
    [InlineData("JOB_ALERT", ImportedEmailType.JobAlert)]
    [InlineData("interview_invitation", ImportedEmailType.InterviewInvitation)] // insensible à la casse
    [InlineData("APPLICATION_REJECTION", ImportedEmailType.ApplicationRejection)]
    public void TryMapMessageType_ShouldMapKnownCodes(string code, ImportedEmailType expected)
    {
        bool success = GmailJobEventMapper.TryMapMessageType(code, out ImportedEmailType result);

        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("SOMETHING_INVALID")]
    [InlineData("")]
    [InlineData(null)]
    public void TryMapMessageType_ShouldFail_ForUnknownOrEmptyCodes(string? code)
    {
        bool success = GmailJobEventMapper.TryMapMessageType(code, out _);

        success.Should().BeFalse();
    }

    [Theory]
    [InlineData("CREATE_APPLICATION", SuggestedActionType.CreateApplication)]
    [InlineData("mark_as_rejected", SuggestedActionType.MarkAsRejected)]
    public void TryMapActionType_ShouldMapKnownCodes(string code, SuggestedActionType expected)
    {
        bool success = GmailJobEventMapper.TryMapActionType(code, out SuggestedActionType result);

        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Fact]
    public void TryMapActionType_ShouldFail_ForUnknownCode()
    {
        bool success = GmailJobEventMapper.TryMapActionType("NOT_A_REAL_ACTION", out _);

        success.Should().BeFalse();
    }
}
