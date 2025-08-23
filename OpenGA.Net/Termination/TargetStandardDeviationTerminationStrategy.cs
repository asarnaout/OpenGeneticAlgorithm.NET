namespace OpenGA.Net.Termination;

public class TargetStandardDeviationTerminationStrategy<T> : BaseTerminationStrategy<T>
{
    private readonly double _targetStandardDeviation;
    private readonly Queue<double> _recentFitnessValues = new();
    private readonly int _window;

    public TargetStandardDeviationTerminationStrategy(double targetStandardDeviation, int window = 5)
    {
        if (targetStandardDeviation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetStandardDeviation), "Target standard deviation must be greater than or equal to 0.");
        }

        if (window <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(window), "Window size must be greater than 0.");
        }

        _targetStandardDeviation = targetStandardDeviation;
        _window = window;
    }

    public override bool Terminate(GeneticAlgorithmState state)
    {
        _recentFitnessValues.Enqueue(state.HighestFitness);

        if (_recentFitnessValues.Count > _window)
        {
            _recentFitnessValues.Dequeue();
        }

        // We need at least 2 values to calculate standard deviation meaningfully
        if (_recentFitnessValues.Count < 2)
        {
            return false;
        }

        var values = _recentFitnessValues.ToArray();
        var mean = values.Average();
        var variance = values.Sum(x => Math.Pow(x - mean, 2)) / values.Length;
        var standardDeviation = Math.Sqrt(variance);

        return standardDeviation <= _targetStandardDeviation;
    }
}
