using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// Provides configuration and registration capabilities for replacement strategies in the genetic algorithm.
/// This class manages the registration of replacement strategies, operator selection policies, and offspring generation rates.
/// 
/// The registration process supports both single and multiple replacement strategies with intelligent
/// defaults applied by OpenGARunner when strategies are not explicitly configured.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class ReplacementStrategyRegistration<T>
{
    private readonly ReplacementStrategyConfiguration<T> _singleReplacementStrategyConfig = new();

    private readonly MultiReplacementStrategyConfiguration<T> _multiReplacementStrategyConfig = new();

    private float? _offspringGenerationRate;

    private bool _isMultiRegistration = false;

    /// <summary>
    /// Registers a single replacement strategy for use in the genetic algorithm.
    /// 
    /// This method is intended for scenarios where only one replacement strategy is needed.
    /// 
    /// If no replacement strategies are registered at all, OpenGARunner defaults to ElitistReplacementStrategy
    /// using this registration method during the DefaultMissingStrategies() process.
    /// </summary>
    /// <param name="singleRegistration">
    /// A configuration action that registers exactly one replacement strategy.
    /// Examples: s => s.Elitist(), s => s.Random()
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when singleRegistration is null</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no replacement strategy is found at the last step of this method's execution.
    /// </exception>
    /// <example>
    /// <code>
    /// .Replacement(r => r.RegisterSingle(s => s.Elitist(0.1f)))
    /// </code>
    /// </example>
    public ReplacementStrategyRegistration<T> RegisterSingle(Action<ReplacementStrategyConfiguration<T>> singleRegistration)
    {
        ArgumentNullException.ThrowIfNull(singleRegistration, nameof(singleRegistration));

        singleRegistration(_singleReplacementStrategyConfig);

        if (_singleReplacementStrategyConfig.ReplacementStrategy is null)
        {
            throw new InvalidOperationException("No replacement strategy was registered.");
        }

        _isMultiRegistration = false;

        return this;
    }

    /// <summary>
    /// Registers multiple replacement strategies for use in the genetic algorithm.
    /// 
    /// This method enables the registration of multiple replacement strategies that will be
    /// selected between during algorithm execution. When multiple strategies are registered,
    /// OpenGARunner applies intelligent operator selection policy defaults:
    /// 
    /// 1. If any strategy has custom weights (> 0), CustomWeightPolicy is automatically applied
    /// 2. If no custom weights and no explicit policy, AdaptivePursuitPolicy is applied by default
    /// 3. If an explicit policy is configured that conflicts with custom weights, an exception is thrown
    /// 
    /// The operator selection policy determines how the algorithm chooses between the registered
    /// replacement strategies during each generation cycle.
    /// </summary>
    /// <param name="configurator">
    /// A configuration action that registers multiple replacement strategies.
    /// Can include custom weights and strategy-specific configurations.
    /// </param>
    /// <returns>
    /// The ReplacementStrategyRegistration instance for method chaining, allowing
    /// further configuration such as OffspringGenerationRate() calls.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when configurator is null</exception>
    /// <example>
    /// <code>
    /// .Replacement(r => r.RegisterMulti(m => m
    ///     .Elitist(0.1f, 0.7f)
    ///     .Tournament(3, true, 0.3f)
    /// ).OverrideOffspringGenerationRate(0.5f))
    /// </code>
    /// </example>
    public ReplacementStrategyRegistration<T> RegisterMulti(Action<MultiReplacementStrategyConfiguration<T>> configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        configurator(_multiReplacementStrategyConfig);
        _isMultiRegistration = true;

        return this;
    }

    /// <summary>
    /// Overrides the offspring generation rate that determines how many offspring are produced relative to the population size.
    /// 
    /// This method allows explicit control over offspring generation, overriding the default behavior where
    /// each replacement strategy defines its own recommended rate. When set, this value takes precedence
    /// over any replacement strategy's RecommendedOffspringGenerationRate property.
    /// 
    /// In OpenGARunner's CalculateOptimalOffspringCount method, the precedence order is:
    /// 1. Custom override value (set by this method) - highest priority
    /// 2. Replacement strategy's RecommendedOffspringGenerationRate - fallback when no override is set
    /// 
    /// The offspring generation rate is a fundamental parameter that controls population dynamics:
    /// - Higher rates (close to 1.0) result in more offspring generation and faster population turnover
    /// - Lower rates (close to 0.0) result in less population change and may slow evolution
    /// - The rate is multiplied by current population size to determine absolute offspring count
    /// 
    /// Example calculations:
    /// - Population size: 100, rate: 0.5 → ~50 offspring generated per generation
    /// - Population size: 200, rate: 0.3 → ~60 offspring generated per generation
    /// 
    /// This rate works in conjunction with crossover rate, mutation rate, and replacement strategy
    /// to control the genetic algorithm's evolutionary pressure and population dynamics.
    /// 
    /// Note: The actual number of offspring may be adjusted by OpenGARunner to respect population
    /// size constraints (minNumberOfChromosomes and maxNumberOfChromosomes).
    /// </summary>
    /// <param name="offspringGenerationRate">
    /// Value between 0 and 1, where:
    /// - 0 indicates no offspring generation (population remains static)
    /// - 1 indicates maximum offspring generation (population size worth of offspring)
    /// - Values > 1 are not allowed as they could lead to exponential population growth
    /// </param>
    /// <returns>
    /// The ReplacementStrategyRegistration instance for method chaining
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when offspringGenerationRate is not between 0 and 1 (inclusive)
    /// </exception>
    /// <example>
    /// <code>
    /// // Override to generate 40% of population size as offspring each generation
    /// .Replacement(r => r.RegisterSingle(s => s.Elitist())
    ///                    .OverrideOffspringGenerationRate(0.4f))
    /// 
    /// // With multiple strategies, still using the same override rate
    /// .Replacement(r => r.RegisterMulti(m => m
    ///     .Elitist().WithCustomWeight(0.7f)
    ///     .Tournament().WithCustomWeight(0.3f)
    /// ).OverrideOffspringGenerationRate(0.6f))
    /// </code>
    /// </example>
    public ReplacementStrategyRegistration<T> OverrideOffspringGenerationRate(float offspringGenerationRate)
    {
        if (offspringGenerationRate < 0 || offspringGenerationRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(offspringGenerationRate), "Value must be between 0 and 1.");
        }

        _offspringGenerationRate = offspringGenerationRate;
        return this;
    }

    internal float? GetOffspringGenerationRateOverride()
    {
        return _offspringGenerationRate;
    }

    internal void ValidateAndDefault()
    {
        if (_isMultiRegistration)
        {
            _multiReplacementStrategyConfig.ValidateAndDefault();
        }
        else
        {
            _singleReplacementStrategyConfig.ValidateAndDefault();
        }
    }

    internal IList<BaseReplacementStrategy<T>> GetRegisteredReplacementStrategies()
    {
        return _isMultiRegistration
            ? _multiReplacementStrategyConfig.ReplacementStrategies
            : [_singleReplacementStrategyConfig.ReplacementStrategy!];
    }

    internal OperatorSelectionPolicy GetReplacementSelectionPolicy()
    {
        return _isMultiRegistration
            ? _multiReplacementStrategyConfig.GetReplacementSelectionPolicy()
            : _singleReplacementStrategyConfig.GetReplacementSelectionPolicy();
    }
}
