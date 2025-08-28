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
    private readonly CrossoverStrategyConfiguration<T> _crossoverStrategyConfig = new();

    private readonly OperatorSelectionPolicyConfiguration _crossoverSelectionPolicyConfig = new();

    private float _crossoverRate = 0.9f;

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
    public void RegisterSingle(Action<CrossoverStrategyConfiguration<T>> singleRegistration)
    {
        ArgumentNullException.ThrowIfNull(singleRegistration, nameof(singleRegistration));

        singleRegistration(_crossoverStrategyConfig);
        
        if (_crossoverStrategyConfig.CrossoverStrategies.Count > 1)
        {
            throw new InvalidOperationException("Multiple crossover strategies registered. Use RegisterMulti for multiple registrations.");
        }
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
    /// .Crossover(c => c.RegisterMulti(m => {
    ///     m.OnePointCrossover().WithCustomWeight(0.6f);
    ///     m.UniformCrossover().WithCustomWeight(0.4f);
    /// }).WithCrossoverRate(0.8f))
    /// </code>
    /// </example>
    public CrossoverStrategyRegistration<T> RegisterMulti(Action<CrossoverStrategyConfiguration<T>> configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        configurator(_crossoverStrategyConfig);

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
    /// - If only one crossover strategy is registered, OpenGARunner will override any policy with FirstChoicePolicy
    /// - If multiple strategies exist without custom weights and no policy is specified, AdaptivePursuitPolicy is applied
    /// - If custom weights are detected without an explicit policy, CustomWeightPolicy is automatically applied
    /// 
    /// Common policies include:
    /// - FirstChoicePolicy: Always selects the first registered strategy (automatic for single strategies)
    /// - RandomChoicePolicy: Randomly selects between strategies with equal probability
    /// - AdaptivePursuitPolicy: Adapts selection based on performance feedback (default for multiple strategies)
    /// - CustomWeightPolicy: Selects based on configured weights (automatic when weights are detected)
    /// - RoundRobinPolicy: Cycles through strategies in order
    /// </summary>
    /// <param name="policyConfigurator">
    /// A configuration action that sets up the operator selection policy.
    /// Examples: p => p.ApplyAdaptivePursuitPolicy(), p => p.ApplyCustomWeightPolicy()
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when policyConfigurator is null</exception>
    /// <exception cref="OperatorSelectionPolicyConflictException">
    /// Thrown by OpenGARunner if custom weights are configured but a non-CustomWeight policy is applied
    /// </exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterMulti(m => {
    ///     m.OnePointCrossover();
    ///     m.UniformCrossover();
    /// }).WithPolicy(p => p.ApplyAdaptivePursuitPolicy()))
    /// </code>
    /// </example>
    public void WithPolicy(Action<OperatorSelectionPolicyConfiguration> policyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(policyConfigurator, nameof(policyConfigurator));

        policyConfigurator(_crossoverSelectionPolicyConfig);
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

    internal IList<BaseCrossoverStrategy<T>> GetRegisteredCrossoverStrategies()
    {
        return _crossoverStrategyConfig.CrossoverStrategies;
    }

    internal OperatorSelectionPolicy GetCrossoverSelectionPolicy()
    {
        return _crossoverSelectionPolicyConfig.Policy;
    }
}
