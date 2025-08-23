namespace OpenGA.Net.Termination;

public abstract class BaseTerminationStrategy<T>
{
    public abstract bool Terminate(OpenGARunner<T> gaRunner);
}
