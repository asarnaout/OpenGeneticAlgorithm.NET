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
    /// <param name="warmupRuns">Number of warm-up runs before adaptation begins (default: 10). These runs are part of the total maximum epochs, not additional to them. During warmup, operators are selected using round-robin to ensure equal initial usage.</param>
    /// <returns>The configured AdaptivePursuitPolicy instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameter values are outside valid ranges</exception>
    public OperatorSelectionPolicy AdaptivePursuit(
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
    public OperatorSelectionPolicy FirstChoice()
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
    public OperatorSelectionPolicy RoundRobin()
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
    public OperatorSelectionPolicy Random()
    {
        var result = new RandomChoicePolicy();
        Policy = result;
        return result;
    }

    /// <summary>
    /// Configures the Custom Weight policy for weighted operator selection.
    /// 
    /// This policy selects operators based on their CustomWeight property using
    /// a weighted roulette wheel algorithm. Operators with higher weights have
    /// a proportionally higher probability of being selected. The weights are
    /// automatically normalized, so they don't need to sum to 1.0.
    /// 
    /// If all operators have zero weight, the policy falls back to uniform selection.
    /// 
    /// Note: This method is optional if you have already assigned custom weights to operators
    /// using the WithCustomWeight() method. In such cases, the CustomWeightPolicy will be
    /// automatically applied during execution, so explicit configuration is not required.
    /// </summary>
    /// <returns>The configured CustomWeightPolicy instance</returns>
    public OperatorSelectionPolicy CustomWeights()
    {
        var result = new CustomWeightPolicy();
        Policy = result;
        return result;
    }

    // Backward compatibility methods - these maintain the old naming convention
    // for existing code that hasn't been updated to the new naming

    /// <summary>
    /// [Obsolete] Use AdaptivePursuit() instead. This method is maintained for backward compatibility.
    /// Configures the Adaptive Pursuit policy for dynamic operator selection.
    /// </summary>
    /// <param name="learningRate">Rate at which probabilities adapt (0.0 to 1.0, default: 0.1)</param>
    /// <param name="minimumProbability">Minimum probability for any operator to ensure exploration (default: 0.05)</param>
    /// <param name="rewardWindowSize">Number of recent rewards to consider for temporal weighting (default: 10)</param>
    /// <param name="diversityWeight">Weight given to diversity bonus in reward calculation (default: 0.1)</param>
    /// <param name="minimumUsageBeforeAdaptation">Minimum times each operator must be used before adaptation begins (default: 5)</param>
    /// <param name="warmupRuns">Number of warm-up runs before adaptation begins (default: 10)</param>
    /// <returns>The configured AdaptivePursuitPolicy instance</returns>
    [Obsolete("Use AdaptivePursuit() instead. This method will be removed in a future version.")]
    public OperatorSelectionPolicy ApplyAdaptivePursuitPolicy(
        double learningRate = 0.1,
        double minimumProbability = 0.05,
        int rewardWindowSize = 10,
        double diversityWeight = 0.1,
        int minimumUsageBeforeAdaptation = 5,
        int warmupRuns = 10)
    {
        return AdaptivePursuit(learningRate, minimumProbability, rewardWindowSize, diversityWeight, minimumUsageBeforeAdaptation, warmupRuns);
    }

    /// <summary>
    /// [Obsolete] Use FirstChoice() instead. This method is maintained for backward compatibility.
    /// Configures the First Choice policy for deterministic operator selection.
    /// </summary>
    /// <returns>The configured FirstChoicePolicy instance</returns>
    [Obsolete("Use FirstChoice() instead. This method will be removed in a future version.")]
    public OperatorSelectionPolicy ApplyFirstChoicePolicy()
    {
        return FirstChoice();
    }

    /// <summary>
    /// [Obsolete] Use RoundRobin() instead. This method is maintained for backward compatibility.
    /// Configures the Round Robin policy for balanced operator selection.
    /// </summary>
    /// <returns>The configured RoundRobinPolicy instance</returns>
    [Obsolete("Use RoundRobin() instead. This method will be removed in a future version.")]
    public OperatorSelectionPolicy ApplyRoundRobinPolicy()
    {
        return RoundRobin();
    }

    /// <summary>
    /// [Obsolete] Use Random() instead. This method is maintained for backward compatibility.
    /// Configures the Random Choice policy for stochastic operator selection.
    /// </summary>
    /// <returns>The configured RandomChoicePolicy instance</returns>
    [Obsolete("Use Random() instead. This method will be removed in a future version.")]
    public OperatorSelectionPolicy ApplyRandomChoicePolicy()
    {
        return Random();
    }

    /// <summary>
    /// [Obsolete] Use CustomWeights() instead. This method is maintained for backward compatibility.
    /// Configures the Custom Weight policy for weighted operator selection.
    /// </summary>
    /// <returns>The configured CustomWeightPolicy instance</returns>
    [Obsolete("Use CustomWeights() instead. This method will be removed in a future version.")]
    public OperatorSelectionPolicy ApplyCustomWeightPolicy()
    {
        return CustomWeights();
    }
}
