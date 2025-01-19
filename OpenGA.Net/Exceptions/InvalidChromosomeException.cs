namespace OpenGA.Net.Exceptions;

public class InvalidChromosomeException : Exception
{
    public InvalidChromosomeException()
    {
    }

    public InvalidChromosomeException(string message)
        : base(message)
    {
    }

    public InvalidChromosomeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
