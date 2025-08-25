namespace OpenGA.Net.Termination;

public class MaximumDurationTerminationStrategy<T> : BaseTerminationStrategy<T>
{
    private readonly TimeSpan _maximumDuration;

    public MaximumDurationTerminationStrategy(TimeSpan maximumDuration)
    {
        if (maximumDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumDuration), "Maximum duration must be greater than zero.");
        }

        _maximumDuration = maximumDuration;
    }

    public override bool Terminate(GeneticAlgorithmState state)
    {
        return state.StopWatch.Elapsed >= _maximumDuration;
    }
}
