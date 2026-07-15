using CareerTrack.Models.Enums;

namespace CareerTrack.Exceptions;

public sealed class InvalidStatusTransitionException : CareerTrackException
{
    public InvalidStatusTransitionException(ApplicationStatus current, ApplicationStatus requested)
        : base($"Le passage de {current} vers {requested} est interdit.")
    {
    }
}
