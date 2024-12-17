namespace OpenGA.Net;

public class MissingInitialPopulationException : Exception
{
    public MissingInitialPopulationException()
    {
    }

    public MissingInitialPopulationException(string message)
        : base(message)
    {
    }

    public MissingInitialPopulationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
