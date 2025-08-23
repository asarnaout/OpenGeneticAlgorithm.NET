namespace OpenGA.Net.Termination;

public class TargetStandardDeviationTerminationStrategy<T>(double targetStandardDeviation, int window = 5) : BaseTerminationStrategy<T>
{
    private readonly double _targetStandardDeviation = targetStandardDeviation;
    private readonly Queue<double> _recentFitnessValues = new();
    private readonly int _window = window;

    public override bool Terminate(OpenGARunner<T> gaRunner)
    {
        _recentFitnessValues.Enqueue(gaRunner.HighestFitness);

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
