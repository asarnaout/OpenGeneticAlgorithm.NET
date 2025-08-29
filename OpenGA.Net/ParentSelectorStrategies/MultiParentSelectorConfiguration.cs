namespace OpenGA.Net.ParentSelectorStrategies;

using OpenGA.Net.Exceptions;
using OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Configuration class specifically for multiple parent selector strategies with weight support.
/// This class provides the same parent selector methods as ParentSelectorConfiguration
/// but with optional weight parameters for use in multi-strategy scenarios.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class MultiParentSelectorConfiguration<T>
{
    internal IList<BaseParentSelectorStrategy<T>> ParentSelectors = [];

    private readonly OperatorSelectionPolicyConfiguration _policyConfig = new();

    /// <summary>
    /// Parents are chosen at random regardless of their fitness.
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiParentSelectorConfiguration<T> Random(float? customWeight = null)
    {
        var result = new RandomParentSelectorStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// The likelihood of an individual chromosome being chosen for mating is proportional to its fitness.
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiParentSelectorConfiguration<T> RouletteWheel(float? customWeight = null)
    {
        var result = new FitnessWeightedRouletteWheelParentSelectorStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// Each iteration, n-individuals are chosen at random to form a tournament and out of this group, 2 individuals are chosen for mating.
    /// The number of tournaments held per iteration is stochastic and depends on the size of the population at each iteration.
    /// </summary>
    /// <param name="stochasticTournament">Defaults to true. If set to true, then the 2 individuals chosen for mating in each 
    /// tournament are the fittest 2 individuals in the tournament, otherwise a roulette wheel is spun to choose the two winners 
    /// out of the n-individuals, where the probability of winning is proportional to each individual's fitness.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiParentSelectorConfiguration<T> Tournament(bool stochasticTournament = true, float? customWeight = null)
    {
        var result = new TournamentParentSelectorStrategy<T>(stochasticTournament);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// Similar to the traditional fitness-weighted roulette wheel selection mechanism, however, Rank Selection
    /// aims to blunt any disproportionate advantage in fitness a chromosome has which will almost always guarantee
    /// its selection over the mid/long term.
    /// 
    /// With Rank Selection, each chromosome's fitness is used to assign it a rank and the rank is used (instead of the absolute
    /// fitness value) to determine the chromosome's advantage in the rouletee wheel. This guarantees that chromosomes with a
    /// disproportionate advantage in fitness will have a (relatively) harder time (compared to the traditional fitness-weighted roulette wheel) 
    /// dominating the selection mechanism.
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiParentSelectorConfiguration<T> Rank(float? customWeight = null)
    {
        var result = new RankSelectionParentSelectorStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// Boltzmann Selection parent selector that uses temperature-based selection probabilities with exponential decay.
    /// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter
    /// that starts at the specified initial value and decays exponentially over epochs: T(t) = T₀ × e^(-α×t).
    /// Higher temperature leads to more uniform selection (exploration), while lower temperature leads to more elitist selection (exploitation).
    /// Exponential decay provides smooth cooling and never reaches absolute zero, maintaining some exploration throughout the run.
    /// </summary>
    /// <param name="temperatureDecayRate">The exponential decay rate per epoch. Higher values (e.g., 0.1) result in faster cooling, 
    /// lower values (e.g., 0.01) result in slower cooling. Must be greater than or equal to 0. Defaults to 0.05.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public MultiParentSelectorConfiguration<T> Boltzmann(double temperatureDecayRate = 0.05, double initialTemperature = 1.0, float? customWeight = null)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }

        var result = new BoltzmannParentSelectorStrategy<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: true);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// Boltzmann Selection parent selector that uses temperature-based selection probabilities with linear decay.
    /// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter
    /// that starts at the specified initial value and decays linearly over epochs: T(t) = T₀ - α×t.
    /// Higher temperature leads to more uniform selection (exploration), while lower temperature leads to more elitist selection (exploitation).
    /// Linear decay provides predictable cooling and can reach zero temperature for pure exploitation in later epochs.
    /// </summary>
    /// <param name="temperatureDecayRate">The linear decay rate per epoch (amount subtracted from temperature each epoch). 
    /// Higher values result in faster cooling. Must be greater than or equal to 0. Defaults to 0.01.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public MultiParentSelectorConfiguration<T> BoltzmannWithLinearDecay(double temperatureDecayRate = 0.01, double initialTemperature = 1.0, float? customWeight = null)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }

        var result = new BoltzmannParentSelectorStrategy<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: false);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// The fittest individual chromosomes are guaranteed to participate in the mating process in the current epoch/generation. Non elites may participate as well.
    /// </summary>
    /// <param name="proportionOfElitesInPopulation">The proportion of elites in the population. Example, if the rate is 0.2 and the population size is 100, then we have 20 elites who are guaranteed to take part in the mating process.</param>
    /// <param name="proportionOfNonElitesAllowedToMate">The proportion of non-elites allowed to take part in the mating process. Non elites are chosen randomly regardless of fitness.</param>
    /// <param name="allowMatingElitesWithNonElites">Defaults to true. Setting this value to false would restrict couples made up of an elite and non-elite members</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiParentSelectorConfiguration<T> Elitist(float proportionOfElitesInPopulation = 0.1f, float proportionOfNonElitesAllowedToMate = 0.01f, bool allowMatingElitesWithNonElites = true, float? customWeight = null)
    {
        if (proportionOfElitesInPopulation <= 0 || proportionOfElitesInPopulation > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfElitesInPopulation), "Value must be greater than 0 and less than or equal to 1.");
        }

        if (proportionOfNonElitesAllowedToMate < 0 || proportionOfNonElitesAllowedToMate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfNonElitesAllowedToMate), "Value must be between 0 and 1.");
        }

        var result = new ElitistParentSelectorStrategy<T>(allowMatingElitesWithNonElites, proportionOfElitesInPopulation, proportionOfNonElitesAllowedToMate);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        ParentSelectors.Add(result);
        return this;
    }

    /// <summary>
    /// Apply a custom strategy for choosing mating parents. Requires an instance of a subclass of <see cref="BaseParentSelectorStrategy<T>">BaseParentSelectorStrategy<T></see>
    /// to dictate which individuals will be chosen to take part in the crossover process.
    /// </summary>
    /// <param name="parentSelector">The custom parent selector instance to add.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiParentSelectorConfiguration<T> Custom(BaseParentSelectorStrategy<T> parentSelector, float? customWeight = null)
    {
        ArgumentNullException.ThrowIfNull(parentSelector, nameof(parentSelector));
        
        if (customWeight.HasValue)
        {
            parentSelector.WithCustomWeight(customWeight.Value);
        }
        
        ParentSelectors.Add(parentSelector);
        return this;
    }

    /// <summary>
    /// Configures the operator selection policy that determines how parent selector strategies are chosen
    /// when multiple strategies are registered.
    /// 
    /// This method allows explicit configuration of the operator selection policy, overriding
    /// OpenGARunner's automatic defaults. However, there are important interaction rules:
    /// 
    /// - If parent selector strategies have custom weights (> 0) but a non-CustomWeight policy is applied,
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
    /// .ParentSelection(p => p.RegisterMulti(m => m
    ///     .Tournament()
    ///     .RouletteWheel()
    ///     .WithPolicy(p => p.AdaptivePursuit())
    /// ))
    /// </code>
    /// </example>
    public MultiParentSelectorConfiguration<T> WithPolicy(Action<OperatorSelectionPolicyConfiguration> policyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(policyConfigurator, nameof(policyConfigurator));

        policyConfigurator(_policyConfig);
        return this;
    }

    internal void ValidateAndDefault()
    {
        if (ParentSelectors is [])
        {
            Tournament();
            _policyConfig.FirstChoice();
        }
        else
        {
            var hasCustomWeights = ParentSelectors.Any(strategy => strategy.CustomWeight > 0);

            if (_policyConfig.Policy is not null)
            {
                if (hasCustomWeights && _policyConfig.Policy is not CustomWeightPolicy)
                {
                    throw new OperatorSelectionPolicyConflictException(
                        @"Cannot apply a non-CustomWeight operator selection policy when parent selector strategies 
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
                // If multiple parent selector strategies and no operator policy specified then default to adaptive pursuit
                _policyConfig.AdaptivePursuit();
            }
        }

        _policyConfig.Policy!.ApplyOperators([..ParentSelectors]);
    }

    internal OperatorSelectionPolicy GetParentSelectorSelectionPolicy()
    {
        return _policyConfig.Policy;
    }
}
