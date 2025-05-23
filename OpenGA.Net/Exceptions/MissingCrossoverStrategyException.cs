namespace OpenGA.Net.Exceptions;

public class MissingCrossoverStrategyException: Exception
{
    public MissingCrossoverStrategyException()
    {
    }

    public MissingCrossoverStrategyException(string message)
        : base(message)
    {
    }

    public MissingCrossoverStrategyException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
