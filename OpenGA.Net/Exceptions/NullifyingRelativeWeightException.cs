namespace OpenGA.Net.Exceptions;

public class NullifyingRelativeWeightException : Exception
{
    public NullifyingRelativeWeightException()
    {
    }

    public NullifyingRelativeWeightException(string message)
        : base(message)
    {
    }

    public NullifyingRelativeWeightException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
