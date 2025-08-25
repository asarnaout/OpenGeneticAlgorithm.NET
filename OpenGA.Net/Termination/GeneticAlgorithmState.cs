using System.Diagnostics;

namespace OpenGA.Net.Termination;

/// <summary>
/// Represents the current state of a genetic algorithm execution, providing essential metrics
/// for termination strategies to evaluate whether the algorithm should continue or terminate.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GeneticAlgorithmState struct.
/// </remarks>
/// <param name="currentEpoch">The current epoch number.</param>
/// <param name="stopwatch">The stopwatch tracking elapsed time since algorithm start.</param>
/// <param name="highestFitness">The highest fitness in the current population.</param>
public readonly struct GeneticAlgorithmState(int currentEpoch, Stopwatch stopwatch, double highestFitness)
{
    /// <summary>
    /// The current epoch/generation number.
    /// </summary>
    public int CurrentEpoch { get; init; } = currentEpoch;

    /// <summary>
    /// The stopwatch tracking elapsed time since the algorithm started.
    /// Use StopWatch.Elapsed to get the current elapsed TimeSpan.
    /// </summary>
    public Stopwatch StopWatch { get; init; } = stopwatch;

    /// <summary>
    /// The highest fitness value found in the current population.
    /// </summary>
    public double HighestFitness { get; init; } = highestFitness;
}
