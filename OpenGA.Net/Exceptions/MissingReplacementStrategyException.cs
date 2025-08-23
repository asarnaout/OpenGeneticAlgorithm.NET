namespace OpenGA.Net.Exceptions;

public class MissingReplacementStrategyException: Exception
{
    public MissingReplacementStrategyException()
    {
    }

    public MissingReplacementStrategyException(string message)
        : base(message)
    {
    }

    public MissingReplacementStrategyException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
