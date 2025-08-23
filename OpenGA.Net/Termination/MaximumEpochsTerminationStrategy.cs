namespace OpenGA.Net.Termination;

internal class MaximumEpochsTerminationStrategy<T> : BaseTerminationStrategy<T>
{
    public override bool Terminate(GeneticAlgorithmState state)
    {
        return state.CurrentEpoch >= state.MaxEpochs;
    }
}
