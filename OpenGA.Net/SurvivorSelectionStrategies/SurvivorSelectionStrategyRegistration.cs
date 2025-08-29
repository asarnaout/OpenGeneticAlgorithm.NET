using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.SurvivorSelectionStrategies;

/// <summary>
/// Provides configuration and registration capabilities for survivor selection strategies in the genetic algorithm.
/// This class manages the registration of survivor selection strategies, operator selection policies, and offspring generation rates.
/// 
/// The registration process supports both single and multiple survivor selection strategies with intelligent
/// defaults applied by OpenGARunner when strategies are not explicitly configured.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class SurvivorSelectionStrategyRegistration<T>
{
    private readonly SurvivorSelectionStrategyConfiguration<T> _singleSurvivorSelectionStrategyConfig = new();

    private readonly MultiSurvivorSelectionStrategyConfiguration<T> _multiSurvivorSelectionStrategyConfig = new();

    private float? _offspringGenerationRate;

    private bool _isMultiRegistration = false;

    /// <summary>
    /// Registers a single survivor selection strategy for use in the genetic algorithm.
    /// 
    /// This method is intended for scenarios where only one survivor selection strategy is needed.
    /// 
    /// If no survivor selection strategies are registered at all, OpenGARunner defaults to ElitistSurvivorSelectionStrategy
    /// using this registration method during the DefaultMissingStrategies() process.
    /// </summary>
    /// <param name="singleRegistration">
    /// A configuration action that registers exactly one survivor selection strategy.
    /// Examples: s => s.Elitist(), s => s.Random()
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when singleRegistration is null</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no survivor selection strategy is found at the last step of this method's execution.
    /// </exception>
    /// <example>
    /// <code>
    /// .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist(0.1f)))
    /// </code>
    /// </example>
    public SurvivorSelectionStrategyRegistration<T> RegisterSingle(Action<SurvivorSelectionStrategyConfiguration<T>> singleRegistration)
    {
        ArgumentNullException.ThrowIfNull(singleRegistration, nameof(singleRegistration));

        singleRegistration(_singleSurvivorSelectionStrategyConfig);

        if (_singleSurvivorSelectionStrategyConfig.SurvivorSelectionStrategy is null)
        {
            throw new InvalidOperationException("No survivor selection strategy was registered.");
        }

        _isMultiRegistration = false;

        return this;
    }

    /// <summary>
    /// Registers multiple survivor selection strategies for use in the genetic algorithm.
    /// 
    /// This method enables the registration of multiple survivor selection strategies that will be
    /// selected between during algorithm execution. When multiple strategies are registered,
    /// OpenGARunner applies intelligent operator selection policy defaults:
    /// 
    /// 1. If any strategy has custom weights (> 0), CustomWeightPolicy is automatically applied
    /// 2. If no custom weights and no explicit policy, AdaptivePursuitPolicy is applied by default
    /// 3. If an explicit policy is configured that conflicts with custom weights, an exception is thrown
    /// 
    /// The operator selection policy determines how the algorithm chooses between the registered
    /// survivor selection strategies during each generation cycle.
    /// </summary>
    /// <param name="configurator">
    /// A configuration action that registers multiple survivor selection strategies.
    /// Can include custom weights and strategy-specific configurations.
    /// </param>
    /// <returns>
    /// The SurvivorSelectionStrategyRegistration instance for method chaining, allowing
    /// further configuration such as OffspringGenerationRate() calls.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when configurator is null</exception>
    /// <example>
    /// <code>
    /// .SurvivorSelection(r => r.RegisterMulti(m => m
    ///     .Elitist(0.1f, 0.7f)
    ///     .Tournament(3, true, 0.3f)
    /// ).OverrideOffspringGenerationRate(0.5f))
    /// </code>
    /// </example>
    public SurvivorSelectionStrategyRegistration<T> RegisterMulti(Action<MultiSurvivorSelectionStrategyConfiguration<T>> configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        configurator(_multiSurvivorSelectionStrategyConfig);
        _isMultiRegistration = true;

        return this;
    }

    /// <summary>
    /// Overrides the offspring generation rate that determines how many offspring are produced relative to the population size.
    /// 
    /// This method allows explicit control over offspring generation, overriding the default behavior where
    /// each survivor selection strategy defines its own recommended rate. When set, this value takes precedence
    /// over any survivor selection strategy's RecommendedOffspringGenerationRate property.
    /// 
    /// In OpenGARunner's CalculateOptimalOffspringCount method, the precedence order is:
    /// 1. Custom override value (set by this method) - highest priority
    /// 2. Survivor selection strategy's RecommendedOffspringGenerationRate - fallback when no override is set
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
    /// This rate works in conjunction with crossover rate, mutation rate, and survivor selection strategy
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
    /// The SurvivorSelectionStrategyRegistration instance for method chaining
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when offspringGenerationRate is not between 0 and 1 (inclusive)
    /// </exception>
    /// <example>
    /// <code>
    /// // Override to generate 40% of population size as offspring each generation
    /// .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist())
    ///                    .OverrideOffspringGenerationRate(0.4f))
    /// 
    /// // With multiple strategies, still using the same override rate
    /// .SurvivorSelection(r => r.RegisterMulti(m => m
    ///     .Elitist().WithCustomWeight(0.7f)
    ///     .Tournament().WithCustomWeight(0.3f)
    /// ).OverrideOffspringGenerationRate(0.6f))
    /// </code>
    /// </example>
    public SurvivorSelectionStrategyRegistration<T> OverrideOffspringGenerationRate(float offspringGenerationRate)
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
            _multiSurvivorSelectionStrategyConfig.ValidateAndDefault();
        }
        else
        {
            _singleSurvivorSelectionStrategyConfig.ValidateAndDefault();
        }
    }

    internal IList<BaseSurvivorSelectionStrategy<T>> GetRegisteredSurvivorSelectionStrategies()
    {
        return _isMultiRegistration
            ? _multiSurvivorSelectionStrategyConfig.SurvivorSelectionStrategies
            : [_singleSurvivorSelectionStrategyConfig.SurvivorSelectionStrategy!];
    }

    internal OperatorSelectionPolicy GetSurvivorSelectionSelectionPolicy()
    {
        return _isMultiRegistration
            ? _multiSurvivorSelectionStrategyConfig.GetSurvivorSelectionSelectionPolicy()
            : _singleSurvivorSelectionStrategyConfig.GetSurvivorSelectionSelectionPolicy();
    }
}
