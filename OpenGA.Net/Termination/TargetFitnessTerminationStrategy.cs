namespace OpenGA.Net.Termination;

public class TargetFitnessTerminationStrategy<T> : BaseTerminationStrategy<T>
{
    private readonly double _targetFitness;

    public TargetFitnessTerminationStrategy(double targetFitness)
    {
        _targetFitness = targetFitness;
    }

    public override bool Terminate(GeneticAlgorithmState state)
    {
        return state.HighestFitness >= _targetFitness;
    }
}
