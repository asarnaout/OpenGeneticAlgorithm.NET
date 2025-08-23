namespace OpenGA.Net.Termination;

internal class MaximumEpochsTerminationStrategy<T> : BaseTerminationStrategy<T>
{
    public override bool Terminate(OpenGARunner<T> gaRunner)
    {
        return gaRunner.CurrentEpoch >= gaRunner.MaxEpochs;
    }
}
