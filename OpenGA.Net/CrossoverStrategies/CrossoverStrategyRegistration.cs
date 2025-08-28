using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.CrossoverStrategies;

/// <summary>
/// Provides configuration and registration capabilities for crossover strategies in the genetic algorithm.
/// This class manages the registration of crossover strategies, operator selection policies, and crossover rates.
/// 
/// The registration process supports both single and multiple crossover strategies with intelligent
/// defaults applied by OpenGARunner when strategies are not explicitly configured.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class CrossoverStrategyRegistration<T>
{
    private readonly CrossoverStrategyConfiguration<T> _singleCrossoverStrategyConfig = new();
    private readonly MultiCrossoverStrategyConfiguration<T> _multiCrossoverStrategyConfig = new();

    private float _crossoverRate = 0.9f;
    private bool _isMultiRegistration = false;

    /// <summary>
    /// Registers a single crossover strategy for use in the genetic algorithm.
    /// 
    /// This method is intended for scenarios where only one crossover strategy is needed.
    /// 
    /// If no crossover strategies are registered at all, OpenGARunner defaults to OnePointCrossover
    /// using this registration method during the DefaultMissingStrategies() process.
    /// </summary>
    /// <param name="singleRegistration">
    /// A configuration action that registers exactly one crossover strategy.
    /// Examples: s => s.OnePointCrossover(), s => s.UniformCrossover()
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when singleRegistration is null</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when multiple crossover strategies are found at the last step of this method's execution.
    /// Use RegisterMulti for multiple strategy registration.
    /// </exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
    /// </code>
    /// </example>
    public CrossoverStrategyRegistration<T> RegisterSingle(Action<CrossoverStrategyConfiguration<T>> singleRegistration)
    {
        ArgumentNullException.ThrowIfNull(singleRegistration, nameof(singleRegistration));

        singleRegistration(_singleCrossoverStrategyConfig);

        if (_singleCrossoverStrategyConfig.CrossoverStrategy is null)
        {
            throw new InvalidOperationException("No crossover strategy was registered.");
        }

        _isMultiRegistration = false;

        return this;
    }

    /// <summary>
    /// Registers multiple crossover strategies for use in the genetic algorithm.
    /// 
    /// This method enables the registration of multiple crossover strategies that will be
    /// selected between during algorithm execution. When multiple strategies are registered,
    /// OpenGARunner applies intelligent operator selection policy defaults:
    /// 
    /// 1. If any strategy has custom weights (> 0), CustomWeightPolicy is automatically applied
    /// 2. If no custom weights and no explicit policy, AdaptivePursuitPolicy is applied by default
    /// 3. If an explicit policy is configured that conflicts with custom weights, an exception is thrown
    /// 
    /// The operator selection policy determines how the algorithm chooses between the registered
    /// crossover strategies during each reproduction cycle.
    /// </summary>
    /// <param name="configurator">
    /// A configuration action that registers multiple crossover strategies.
    /// Can include custom weights and strategy-specific configurations.
    /// </param>
    /// <returns>
    /// The CrossoverStrategyRegistration instance for method chaining, allowing
    /// further configuration such as WithCrossoverRate() or WithPolicy() calls.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when configurator is null</exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterMulti(m => m
    ///     .OnePointCrossover(0.6f)
    ///     .UniformCrossover(0.4f)
    ///     .WithPolicy(p => p.ApplyAdaptivePursuitPolicy())
    /// ).WithCrossoverRate(0.8f))
    /// </code>
    /// </example>
    public CrossoverStrategyRegistration<T> RegisterMulti(Action<MultiCrossoverStrategyConfiguration<T>> configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        configurator(_multiCrossoverStrategyConfig);
        _isMultiRegistration = true;

        return this;
    }

    /// <summary>
    /// Sets the global crossover rate that determines the likelihood that a selected couple will produce offspring.
    /// 
    /// During each generation, a random number is compared against this rate to decide if crossover occurs.
    /// Individual strategies can override this value using BaseCrossoverStrategy.OverrideCrossoverRate().
    /// 
    /// - Higher values (near 1.0) produce more offspring and speed up convergence
    /// - Lower values (near 0.0) reduce genetic mixing and may slow evolution
    /// - Default is 0.9 (90%)
    /// </summary>
    /// <param name="crossoverRate">
    /// A value between 0 and 1 (inclusive). 0 means never crossover; 1 means always crossover.
    /// </param>
    /// <returns>The CrossoverStrategyRegistration instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside [0,1].</exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()).WithCrossoverRate(0.8f))
    /// </code>
    /// </example>
    public CrossoverStrategyRegistration<T> WithCrossoverRate(float crossoverRate)
    {
        if (crossoverRate < 0 || crossoverRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(crossoverRate), "Value must be between 0 and 1.");
        }

        _crossoverRate = crossoverRate;
        return this;
    }

    internal float GetCrossoverRate()
    {
        return _crossoverRate;
    }

    internal void ValidateAndDefault()
    {
        if (_isMultiRegistration)
        {
            _multiCrossoverStrategyConfig.ValidateAndDefault();
        }
        else
        {
            _singleCrossoverStrategyConfig.ValidateAndDefault();
        }
    }

    internal OperatorSelectionPolicy GetCrossoverSelectionPolicy()
    {
        return _isMultiRegistration
            ? _multiCrossoverStrategyConfig.GetCrossoverSelectionPolicy()
            : _singleCrossoverStrategyConfig.GetCrossoverSelectionPolicy();
    }
}
