namespace CareerTrack.Exceptions;

public abstract class CareerTrackException : Exception
{
    protected CareerTrackException(string message) : base(message)
    {
    }
}
