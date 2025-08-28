namespace OpenGA.Net.Termination;

public class TargetFitnessTerminationStrategy<T>(double targetFitness) : BaseTerminationStrategy<T>
{
    private readonly double _targetFitness = targetFitness;

    public override bool Terminate(GeneticAlgorithmState state)
    {
        return state.HighestFitness >= _targetFitness;
    }
}
