namespace OpenGA.Net.Termination;

public class MaximumEpochsTerminationStrategy
{
    private readonly int _maxEpochs;

    public MaximumEpochsTerminationStrategy(int maxEpochs)
    {
        if (maxEpochs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEpochs), "Maximum epochs must be greater than 0.");
        }

        _maxEpochs = maxEpochs;
    }

    public bool Terminate(int epoch)
    {
        return epoch >= _maxEpochs;
    }
}
