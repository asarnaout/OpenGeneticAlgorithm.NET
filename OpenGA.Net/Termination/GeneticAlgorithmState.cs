namespace OpenGA.Net.Termination;

/// <summary>
/// Represents the current state of a genetic algorithm execution, providing essential metrics
/// for termination strategies to evaluate whether the algorithm should continue or terminate.
/// </summary>
public struct GeneticAlgorithmState
{
    public GeneticAlgorithmState()
    {
    }

    /// <summary>
    /// The current epoch/generation number.
    /// </summary>
    public int CurrentEpoch { get; internal set; }

    /// <summary>
    /// The maximum number of epochs the algorithm should run.
    /// </summary>
    public int MaxEpochs { get; internal set; } = 80;

    /// <summary>
    /// The elapsed time since the algorithm started.
    /// </summary>
    public TimeSpan CurrentDuration { get; internal set; } = TimeSpan.Zero;

    /// <summary>
    /// The highest fitness value found in the current population.
    /// </summary>
    public double HighestFitness { get; internal set; }
}
