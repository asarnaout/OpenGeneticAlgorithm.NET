namespace OpenGA.Net.Termination;

internal class MaximumEpochsTerminationStrategy<T>(int maxEpochs) : BaseTerminationStrategy<T>
{
    private readonly int _maxEpochs = maxEpochs;
    
    public override bool Terminate(GeneticAlgorithmState state)
    {
        return state.CurrentEpoch >= _maxEpochs;
    }
}
