namespace OpenGA.Net.Termination;

public class TerminationStrategyConfiguration<T>
{
    internal List<BaseTerminationStrategy<T>> TerminationStrategies { get; } = [];

    /// <summary>
    /// Adds a termination strategy that stops the genetic algorithm when the maximum number of epochs is reached.
    /// </summary>
    /// <returns>The configured termination strategy instance.</returns>
    public BaseTerminationStrategy<T> MaximumEpochs(int maxEpochs)
    {
        if (maxEpochs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEpochs), "Maximum epochs must be greater than zero.");  
        }            

        var strategy = new MaximumEpochsTerminationStrategy<T>(maxEpochs);
        TerminationStrategies.Add(strategy);
        return strategy;
    }

    /// <summary>
    /// Adds a termination strategy that stops the genetic algorithm when the maximum duration is reached.
    /// </summary>
    /// <param name="maximumDuration">The maximum time the algorithm should run before terminating.</param>
    /// <returns>The configured termination strategy instance.</returns>
    public BaseTerminationStrategy<T> MaximumDuration(TimeSpan maximumDuration)
    {
        var strategy = new MaximumDurationTerminationStrategy<T>(maximumDuration);
        TerminationStrategies.Add(strategy);
        return strategy;
    }

    /// <summary>
    /// Adds a termination strategy that stops the genetic algorithm when the standard deviation of recent fitness values
    /// falls below a specified threshold, indicating convergence.
    /// </summary>
    /// <param name="targetStandardDeviation">The minimum standard deviation threshold. When the standard deviation of recent fitness values falls below this value, the algorithm terminates.</param>
    /// <param name="window">The number of recent fitness values to track for calculating standard deviation. Defaults to 5.</param>
    /// <returns>The configured termination strategy instance.</returns>
    public BaseTerminationStrategy<T> TargetStandardDeviation(double targetStandardDeviation, int window = 5)
    {
        var strategy = new TargetStandardDeviationTerminationStrategy<T>(targetStandardDeviation, window);
        TerminationStrategies.Add(strategy);
        return strategy;
    }

    /// <summary>
    /// Adds a custom termination strategy. Requires an instance of a subclass of <see cref="BaseTerminationStrategy{T}">BaseTerminationStrategy&lt;T&gt;</see>
    /// to dictate when the genetic algorithm should terminate.
    /// </summary>
    /// <param name="terminationStrategy">The custom termination strategy to add.</param>
    /// <returns>The provided termination strategy instance.</returns>
    public BaseTerminationStrategy<T> Custom(BaseTerminationStrategy<T> terminationStrategy)
    {
        ArgumentNullException.ThrowIfNull(terminationStrategy, nameof(terminationStrategy));
        TerminationStrategies.Add(terminationStrategy);
        return terminationStrategy;
    }

    /// <summary>
    /// Checks if any of the configured termination strategies indicate that the algorithm should terminate.
    /// </summary>
    /// <param name="state">The current state of the genetic algorithm to evaluate.</param>
    /// <returns>True if any termination strategy indicates termination should occur, false otherwise.</returns>
    internal bool ShouldTerminate(GeneticAlgorithmState state)
    {
        return TerminationStrategies.Any(strategy => strategy.Terminate(state));
    }
}
