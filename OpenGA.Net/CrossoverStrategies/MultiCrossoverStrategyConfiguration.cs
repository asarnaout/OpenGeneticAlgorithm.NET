namespace OpenGA.Net.CrossoverStrategies;

using OpenGA.Net.Exceptions;
using OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Configuration class specifically for multiple crossover strategies with weight support.
/// This class provides the same crossover strategy methods as CrossoverStrategyConfiguration
/// but with optional weight parameters for use in multi-strategy scenarios.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class MultiCrossoverStrategyConfiguration<T>
{
    internal IList<BaseCrossoverStrategy<T>> CrossoverStrategies = [];

    private readonly OperatorSelectionPolicyConfiguration _policyConfig = new();

    /// <summary>
    /// A point is chosen at random, and all the genes following that point are swapped between both parent chromosomes to produce two new child chromosomes
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> OnePointCrossover(float? customWeight = null)
    {
        var result = new OnePointCrossoverStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        CrossoverStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// A child chromosome is created by copying gene by gene from either parents (on a random basis).
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> UniformCrossover(float? customWeight = null)
    {
        var result = new UniformCrossoverStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        CrossoverStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Multiple points are chosen at random, and genes are alternated between parents at each crossover point to produce two new child chromosomes.
    /// </summary>
    /// <param name="numberOfPoints">The number of crossover points to use. Must be greater than 0.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> KPointCrossover(int numberOfPoints, float? customWeight = null)
    {
        if (numberOfPoints <= 0)
        {
            throw new ArgumentException("Number of crossover points must be greater than 0.", nameof(numberOfPoints));
        }

        var result = new KPointCrossoverStrategy<T>(numberOfPoints);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        CrossoverStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply a custom strategy for crossing over chromosomes. Requires an instance of a subclass of <see cref="BaseCrossoverStrategy<T>">BaseCrossoverStrategy<T></see>
    /// to dictate which how a Couple of Chromosomes can reproduce a new set of Chromosomes.
    /// </summary>
    /// <param name="crossoverStrategy">The custom crossover strategy instance to add.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> CustomCrossover(BaseCrossoverStrategy<T> crossoverStrategy, float? customWeight = null)
    {
        ArgumentNullException.ThrowIfNull(crossoverStrategy, nameof(crossoverStrategy));
        
        if (customWeight.HasValue)
        {
            crossoverStrategy.WithCustomWeight(customWeight.Value);
        }
        
        CrossoverStrategies.Add(crossoverStrategy);
        return this;
    }

    /// <summary>
    /// Configures the operator selection policy that determines how crossover strategies are chosen
    /// when multiple strategies are registered.
    /// 
    /// This method allows explicit configuration of the operator selection policy, overriding
    /// OpenGARunner's automatic defaults. However, there are important interaction rules:
    /// 
    /// - If crossover strategies have custom weights (> 0) but a non-CustomWeight policy is applied,
    ///   OpenGARunner will throw an OperatorSelectionPolicyConflictException during DefaultMissingStrategies()
    /// - If multiple strategies exist without custom weights and no policy is specified, AdaptivePursuitPolicy is applied
    /// - If custom weights are detected without an explicit policy, CustomWeightPolicy is automatically applied
    /// 
    /// Common policies include:
    /// - RandomChoicePolicy: Randomly selects between strategies with equal probability
    /// - AdaptivePursuitPolicy: Adapts selection based on performance feedback (default for multiple strategies)
    /// - CustomWeightPolicy: Selects based on configured weights (automatic when weights are detected)
    /// - RoundRobinPolicy: Cycles through strategies in order
    /// </summary>
    /// <param name="policyConfigurator">
    /// A configuration action that sets up the operator selection policy.
    /// Examples: p => p.AdaptivePursuit(), p => p.CustomWeights()
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when policyConfigurator is null</exception>
    /// <exception cref="OperatorSelectionPolicyConflictException">
    /// Thrown by OpenGARunner if custom weights are configured but a non-CustomWeight policy is applied
    /// </exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterMulti(m => m
    ///     .OnePointCrossover()
    ///     .UniformCrossover()
    ///     .WithPolicy(p => p.AdaptivePursuit())
    /// ))
    /// </code>
    /// </example>
    public MultiCrossoverStrategyConfiguration<T> WithPolicy(Action<OperatorSelectionPolicyConfiguration> policyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(policyConfigurator, nameof(policyConfigurator));

        policyConfigurator(_policyConfig);
        return this;
    }

    internal void ValidateAndDefault()
    {
        if (CrossoverStrategies is [])
        {
            OnePointCrossover();
            _policyConfig.FirstChoice();
        }
        else
        {
            var hasCustomWeights = CrossoverStrategies.Any(strategy => strategy.CustomWeight > 0);

            if (_policyConfig.Policy is not null)
            {
                if (hasCustomWeights && _policyConfig.Policy is not CustomWeightPolicy)
                {
                    throw new OperatorSelectionPolicyConflictException(
                        @"Cannot apply a non-CustomWeight operator selection policy when crossover strategies 
                            have custom weights. Either remove the custom weights using WithCustomWeight(0) or use 
                            CustomWeights().");
                }
            }
            else if (hasCustomWeights)
            {
                // Auto-apply CustomWeightPolicy when weights are detected and no policy is explicitly set
                _policyConfig.CustomWeights();
            }
            else
            {
                // If multiple crossover strategies and no operator policy specified then default to adaptive pursuit
                _policyConfig.AdaptivePursuit();
            }
        }

        _policyConfig.Policy!.ApplyOperators([..CrossoverStrategies]);
    }

    internal OperatorSelectionPolicy GetCrossoverSelectionPolicy()
    {
        return _policyConfig.Policy;
    }
}
