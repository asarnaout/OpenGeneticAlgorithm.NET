namespace OpenGA.Net.Termination;

/// <summary>
/// Represents the current state of a genetic algorithm execution, providing essential metrics
/// for termination strategies to evaluate whether the algorithm should continue or terminate.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GeneticAlgorithmState struct.
/// </remarks>
/// <param name="currentEpoch">The current epoch number.</param>
/// <param name="currentDuration">The elapsed time since algorithm start.</param>
/// <param name="highestFitness">The highest fitness in the current population.</param>
public readonly struct GeneticAlgorithmState(int currentEpoch, TimeSpan currentDuration, double highestFitness)
{
    /// <summary>
    /// The current epoch/generation number.
    /// </summary>
    public int CurrentEpoch { get; init; } = currentEpoch;

    /// <summary>
    /// The elapsed time since the algorithm started.
    /// </summary>
    public TimeSpan CurrentDuration { get; init; } = currentDuration;

    /// <summary>
    /// The highest fitness value found in the current population.
    /// </summary>
    public double HighestFitness { get; init; } = highestFitness;
}
