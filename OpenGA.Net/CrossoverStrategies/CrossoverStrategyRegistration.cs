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
    /// further configuration such as Rate() or WithPolicy() calls.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when configurator is null</exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterMulti(m => {
    ///     m.OnePointCrossover().WithCustomWeight(0.6f);
    ///     m.UniformCrossover().WithCustomWeight(0.4f);
    /// }).Rate(0.8f))
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
    /// Sets the crossover rate that determines the likelihood of two mating parents producing offspring.
    /// 
    /// The crossover rate is a fundamental parameter in genetic algorithms that controls reproduction
    /// frequency. During each generation, when a couple is selected for mating, a random number is
    /// generated and compared against this rate to determine if crossover should occur.
    /// 
    /// - Higher rates (close to 1.0) result in more offspring generation and faster convergence
    /// - Lower rates (close to 0.0) result in less genetic mixing and may slow evolution
    /// - The default value of 0.9 (90%) is commonly used and provides a good balance
    /// 
    /// This rate works in conjunction with the mutation rate and replacement strategy to control
    /// the genetic algorithm's evolutionary pressure and population dynamics.
    /// </summary>
    /// <param name="crossoverRate">
    /// Value between 0 and 1, where:
    /// - 0 indicates no chance of crossover (no offspring will be produced)
    /// - 1 indicates 100% chance of crossover (all selected couples will produce offspring)
    /// - Default: 0.9 (90% chance)
    /// </param>
    /// <returns>
    /// The CrossoverStrategyRegistration instance for method chaining
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when crossoverRate is not between 0 and 1 (inclusive)
    /// </exception>
    /// <example>
    /// <code>
    /// .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()).Rate(0.8f))
    /// </code>
    /// </example>
    public CrossoverStrategyRegistration<T> Rate(float crossoverRate)
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
