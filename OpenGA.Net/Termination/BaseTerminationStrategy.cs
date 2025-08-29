namespace OpenGA.Net.Termination;

/// <summary>
/// Abstract base class for all termination strategies in the genetic algorithm.
/// Termination strategies define the conditions under which the genetic algorithm
/// should stop evolving and return the best solution found so far.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
/// <remarks>
/// Termination strategies are crucial for controlling the genetic algorithm's execution
/// and determining when an acceptable solution has been found or when further evolution
/// is unlikely to yield significant improvements.
/// 
/// Common termination criteria include:
/// - **Time-based**: Maximum execution duration or wall-clock time limits
/// - **Generation-based**: Maximum number of epochs/generations
/// - **Fitness-based**: Target fitness value reached or fitness improvement stagnation
/// - **Convergence-based**: Population diversity below threshold or fitness variance minimal
/// - **Resource-based**: Memory usage, computational budget, or evaluation count limits
/// 
/// Multiple termination strategies can be combined, with the algorithm terminating
/// when any of the configured strategies indicates termination should occur.
/// </remarks>
public abstract class BaseTerminationStrategy<T>
{
    /// <summary>
    /// Abstract method that must be implemented by concrete termination strategies to define
    /// the specific conditions under which the genetic algorithm should terminate.
    /// This method is called after each generation to determine if evolution should continue.
    /// </summary>
    /// <param name="state">The current state of the genetic algorithm containing all relevant
    /// information for making termination decisions. This includes:
    /// - Current epoch/generation number
    /// - Elapsed execution time via StopWatch
    /// - Highest fitness value achieved so far
    /// - Population statistics and diversity metrics
    /// - Historical performance data for trend analysis</param>
    /// <returns>
    /// <c>true</c> if the genetic algorithm should terminate and return the current best solution;
    /// <c>false</c> if evolution should continue for additional generations.
    /// </returns>
    /// <remarks>
    /// Implementation guidelines for concrete termination strategies:
    /// 
    /// 1. **Stateless vs Stateful**: Consider whether the strategy needs to maintain internal state
    ///    between calls (e.g., tracking fitness history for convergence detection)
    /// 
    /// 2. **Performance**: Keep termination checks lightweight as they're called every generation.
    ///    Avoid expensive computations that could significantly impact algorithm performance.
    /// 
    /// 3. **Robustness**: Handle edge cases gracefully (e.g., zero elapsed time, identical fitness values)
    /// 
    /// 4. **Determinism**: For reproducible results, avoid relying on external factors that might
    ///    vary between runs (unless explicitly intended for adaptive behavior)
    /// 
    /// 5. **Validation**: Ensure termination criteria are mathematically sound and won't cause
    ///    premature termination or infinite loops
    /// 
    /// Examples of termination logic:
    /// - **Maximum Epochs**: <c>return state.CurrentEpoch >= maxEpochs;</c>
    /// - **Target Fitness**: <c>return state.HighestFitness >= targetFitness;</c>
    /// - **Time Limit**: <c>return state.StopWatch.Elapsed >= maxDuration;</c>
    /// - **Convergence**: Track fitness standard deviation over recent generations
    /// </remarks>
    public abstract bool Terminate(GeneticAlgorithmState state);
}
