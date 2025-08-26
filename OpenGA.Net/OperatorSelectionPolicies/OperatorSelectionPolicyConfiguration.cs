namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Configuration class for setting up operator selection policies in the genetic algorithm.
/// 
/// This class provides a fluent API for configuring how operators (such as crossover strategies)
/// are selected during the evolution process. It supports various selection policies including
/// adaptive learning algorithms and deterministic selection strategies.
/// </summary>
public class OperatorSelectionPolicyConfiguration
{
    internal OperatorSelectionPolicy Policy = default!;

    /// <summary>
    /// Configures the Adaptive Pursuit policy for dynamic operator selection.
    /// 
    /// Adaptive Pursuit learns which operators perform best over time and adaptively
    /// increases the probability of selecting high-performing operators while maintaining
    /// minimum exploration of all available operators.
    /// </summary>
    /// <param name="learningRate">Rate at which probabilities adapt (0.0 to 1.0, default: 0.1). Higher values adapt faster but may be less stable.</param>
    /// <param name="minimumProbability">Minimum probability for any operator to ensure exploration (default: 0.05). Prevents any operator from being completely ignored.</param>
    /// <param name="rewardWindowSize">Number of recent rewards to consider for temporal weighting (default: 10). Larger windows provide more stable but slower adaptation.</param>
    /// <param name="diversityWeight">Weight given to diversity bonus in reward calculation (default: 0.1). Encourages operators that maintain population diversity.</param>
    /// <param name="minimumUsageBeforeAdaptation">Minimum times each operator must be used before adaptation begins (default: 5). Ensures fair initial evaluation.</param>
    /// <param name="warmupRuns">Number of warm-up runs before adaptation begins (default: 10). Allows the algorithm to gather initial performance data.</param>
    /// <returns>The configured AdaptivePursuitPolicy instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameter values are outside valid ranges</exception>
    public OperatorSelectionPolicy ApplyAdaptivePursuitPolicy(
        double learningRate = 0.1,
        double minimumProbability = 0.05,
        int rewardWindowSize = 10,
        double diversityWeight = 0.1,
        int minimumUsageBeforeAdaptation = 5,
        int warmupRuns = 10)
    {
        if (learningRate < 0.0 || learningRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be between 0.0 and 1.0.");
        }

        if (minimumProbability < 0.0 || minimumProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumProbability), "Minimum probability must be between 0.0 and 1.0.");
        }

        if (warmupRuns < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(warmupRuns), "Warm-up runs must be non-negative.");
        }

        var result = new AdaptivePursuitPolicy(
            learningRate,
            minimumProbability,
            rewardWindowSize,
            diversityWeight,
            minimumUsageBeforeAdaptation,
            warmupRuns);

        Policy = result;
        return result;
    }

    /// <summary>
    /// Configures the First Choice policy for deterministic operator selection.
    /// 
    /// This policy always selects the first operator from the available list, providing
    /// predictable and consistent behavior. It's automatically applied when only one
    /// operator is configured, and can be explicitly set when deterministic selection
    /// is preferred over adaptive strategies.
    /// </summary>
    /// <returns>The configured FirstChoicePolicy instance</returns>
    public OperatorSelectionPolicy ApplyFirstChoicePolicy()
    {
        var result = new FirstChoicePolicy();
        Policy = result;
        return result;
    }

    /// <summary>
    /// Configures the Round Robin policy for balanced operator selection.
    /// 
    /// This policy cycles through all available operators in sequence, ensuring each
    /// operator is used an equal number of times. It provides predictable rotation
    /// behavior and fair distribution of operator usage without any bias towards
    /// specific operators.
    /// </summary>
    /// <returns>The configured RoundRobinPolicy instance</returns>
    public OperatorSelectionPolicy ApplyRoundRobinPolicy()
    {
        var result = new RoundRobinPolicy();
        Policy = result;
        return result;
    }

    /// <summary>
    /// Configures the Random Choice policy for stochastic operator selection.
    /// 
    /// This policy randomly selects operators with equal probability, introducing
    /// randomness into operator selection. It provides statistical fairness over
    /// many selections and helps avoid systematic biases that might emerge from
    /// deterministic selection patterns.
    /// </summary>
    /// <returns>The configured RandomChoicePolicy instance</returns>
    public OperatorSelectionPolicy ApplyRandomChoicePolicy()
    {
        var result = new RandomChoicePolicy();
        Policy = result;
        return result;
    }
}
