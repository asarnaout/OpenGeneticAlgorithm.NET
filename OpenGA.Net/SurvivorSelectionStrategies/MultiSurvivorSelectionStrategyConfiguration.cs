namespace OpenGA.Net.SurvivorSelectionStrategies;

using OpenGA.Net.Exceptions;
using OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Configuration class specifically for multiple survivor selection strategies with weight support.
/// This class provides the same survivor selection strategy methods as SurvivorSelectionStrategyConfiguration
/// but with optional weight parameters for use in multi-strategy scenarios.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class MultiSurvivorSelectionStrategyConfiguration<T>
{
    internal IList<BaseSurvivorSelectionStrategy<T>> SurvivorSelectionStrategies = [];

    private readonly OperatorSelectionPolicyConfiguration _policyConfig = new();

    /// <summary>
    /// Apply random elimination survivor selection strategy. Eliminates chromosomes randomly from the population 
    /// to make room for offspring, ensuring population size is maintained.
    /// Each chromosome has an equal chance of being eliminated.
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiSurvivorSelectionStrategyConfiguration<T> Random(float? customWeight = null)
    {
        var result = new RandomEliminationSurvivorSelectionStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply generational survivor selection strategy. Completely replaces the entire parent population with offspring.
    /// In this strategy, no parent chromosomes survive to the next generation - the entire population
    /// is renewed with the offspring generation.
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiSurvivorSelectionStrategyConfiguration<T> Generational(float? customWeight = null)
    {
        var result = new GenerationalSurvivorSelectionStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply elitist survivor selection strategy. Protects the top-performing chromosomes (elites) from elimination
    /// based on their fitness values, while allowing the remaining population to be replaced with offspring.
    /// This ensures that the best solutions are preserved across generations.
    /// </summary>
    /// <param name="elitePercentage">
    /// The percentage of the population to protect as elites (0.0 to 1.0).
    /// Default is 0.1 (10%). Must be between 0.0 and 1.0.
    /// </param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when elitePercentage is not between 0.0 and 1.0.
    /// </exception>
    public MultiSurvivorSelectionStrategyConfiguration<T> Elitist(float elitePercentage = 0.1f, float? customWeight = null)
    {
        if (elitePercentage < 0.0f || elitePercentage > 1.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elitePercentage), 
                elitePercentage, 
                "Elite percentage must be between 0.0 and 1.0.");
        }

        var result = new ElitistSurvivorSelectionStrategy<T>(elitePercentage);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply tournament-based survivor selection strategy. Eliminates chromosomes through competitive tournaments
    /// where the least fit individuals are more likely to be eliminated.
    /// </summary>
    /// <param name="tournamentSize">
    /// The number of chromosomes that participate in each tournament. Must be at least 3.
    /// Larger tournaments increase selection pressure towards eliminating less fit chromosomes.
    /// </param>
    /// <param name="stochasticTournament">
    /// If true, uses weighted random selection within tournaments based on inverse fitness.
    /// If false, always eliminates the least fit chromosome in each tournament.
    /// </param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when tournamentSize is less than 3.
    /// </exception>
    public MultiSurvivorSelectionStrategyConfiguration<T> Tournament(int tournamentSize = 3, bool stochasticTournament = true, float? customWeight = null)
    {
        if (tournamentSize < 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tournamentSize), 
                tournamentSize, 
                "Tournament size must be at least 3.");
        }

        var result = new TournamentSurvivorSelectionStrategy<T>(tournamentSize, stochasticTournament);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply age-based survivor selection strategy. Eliminates chromosomes based on their age using a weighted
    /// roulette wheel where older chromosomes have higher probability of being eliminated.
    /// This encourages population turnover while maintaining some genetic diversity.
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiSurvivorSelectionStrategyConfiguration<T> AgeBased(float? customWeight = null)
    {
        var result = new AgeBasedSurvivorSelectionStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply a custom survivor selection strategy. Requires an instance of a subclass of <see cref="BaseSurvivorSelectionStrategy<T>">BaseSurvivorSelectionStrategy<T></see>
    /// to dictate how chromosomes are eliminated from the population to make room for offspring.
    /// </summary>
    /// <param name="survivorSelectionStrategy">The custom survivor selection strategy to apply</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiSurvivorSelectionStrategyConfiguration<T> Custom(BaseSurvivorSelectionStrategy<T> survivorSelectionStrategy, float? customWeight = null)
    {
        ArgumentNullException.ThrowIfNull(survivorSelectionStrategy, nameof(survivorSelectionStrategy));
        
        if (customWeight.HasValue)
        {
            survivorSelectionStrategy.WithCustomWeight(customWeight.Value);
        }
        
        SurvivorSelectionStrategies.Add(survivorSelectionStrategy);
        return this;
    }

    /// <summary>
    /// Apply Boltzmann survivor selection strategy that uses temperature-based elimination probabilities with exponential decay.
    /// This strategy applies the Boltzmann distribution to control elimination pressure through a temperature parameter
    /// that starts at the specified initial value and decays exponentially over epochs: T(t) = T₀ × e^(-α×t).
    /// Higher temperature leads to more uniform elimination (exploration), while lower temperature leads to more fitness-based elimination (exploitation).
    /// Uses inverse fitness weighting where chromosomes with lower fitness have higher probability of elimination.
    /// </summary>
    /// <param name="temperatureDecayRate">The exponential decay rate per epoch. Higher values (e.g., 0.1) result in faster cooling, 
    /// lower values (e.g., 0.01) result in slower cooling. Must be greater than or equal to 0. Defaults to 0.05.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public MultiSurvivorSelectionStrategyConfiguration<T> Boltzmann(double temperatureDecayRate = 0.05, double initialTemperature = 1.0, float? customWeight = null)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }
        
        var result = new BoltzmannSurvivorSelectionStrategy<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: true);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply Boltzmann survivor selection strategy that uses temperature-based elimination probabilities with linear decay.
    /// This strategy applies the Boltzmann distribution to control elimination pressure through a temperature parameter
    /// that starts at the specified initial value and decays linearly over epochs: T(t) = T₀ - α×t.
    /// Higher temperature leads to more uniform elimination (exploration), while lower temperature leads to more fitness-based elimination (exploitation).
    /// Uses inverse fitness weighting where chromosomes with lower fitness have higher probability of elimination.
    /// </summary>
    /// <param name="temperatureDecayRate">The linear decay rate per epoch (amount subtracted from temperature each epoch). 
    /// Higher values result in faster cooling. Must be greater than or equal to 0. Defaults to 0.01.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public MultiSurvivorSelectionStrategyConfiguration<T> BoltzmannWithLinearDecay(double temperatureDecayRate = 0.01, double initialTemperature = 1.0, float? customWeight = null)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }
        
        var result = new BoltzmannSurvivorSelectionStrategy<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: false);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        SurvivorSelectionStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Configures the operator selection policy that determines how survivor selection strategies are chosen
    /// when multiple strategies are registered.
    /// 
    /// This method allows explicit configuration of the operator selection policy, overriding
    /// OpenGARunner's automatic defaults. However, there are important interaction rules:
    /// 
    /// - If survivor selection strategies have custom weights (> 0) but a non-CustomWeight policy is applied,
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
    /// .SurvivorSelection(r => r.RegisterMulti(m => m
    ///     .Elitist(0.1f)
    ///     .Tournament(3)
    ///     .WithPolicy(p => p.AdaptivePursuit())
    /// ))
    /// </code>
    /// </example>
    public MultiSurvivorSelectionStrategyConfiguration<T> WithPolicy(Action<OperatorSelectionPolicyConfiguration> policyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(policyConfigurator, nameof(policyConfigurator));

        policyConfigurator(_policyConfig);
        return this;
    }

    internal void ValidateAndDefault(Random random)
    {
        if (SurvivorSelectionStrategies is [])
        {
            Elitist();
            _policyConfig.FirstChoice();
        }
        else
        {
            var hasCustomWeights = SurvivorSelectionStrategies.Any(strategy => strategy.CustomWeight > 0);

            if (_policyConfig.Policy is not null)
            {
                if (hasCustomWeights && _policyConfig.Policy is not CustomWeightPolicy)
                {
                    throw new OperatorSelectionPolicyConflictException(
                        @"Cannot apply a non-CustomWeight operator selection policy when survivor selection strategies 
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
                // If multiple survivor selection strategies and no operator policy specified then default to adaptive pursuit
                _policyConfig.AdaptivePursuit();
            }
        }

        _policyConfig.Policy!.ApplyOperators([..SurvivorSelectionStrategies], random);
    }

    internal OperatorSelectionPolicy GetSurvivorSelectionSelectionPolicy()
    {
        return _policyConfig.Policy;
    }
}
