namespace OpenGA.Net.Exceptions;

public class MissingReproductionSelectorsException : Exception
{
    public MissingReproductionSelectorsException()
    {
    }

    public MissingReproductionSelectorsException(string message)
        : base(message)
    {
    }

    public MissingReproductionSelectorsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
