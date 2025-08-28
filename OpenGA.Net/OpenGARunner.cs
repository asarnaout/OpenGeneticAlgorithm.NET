using OpenGA.Net.Exceptions;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;
using OpenGA.Net.Termination;
using OpenGA.Net.OperatorSelectionPolicies;
using OpenGA.Net.Extensions;
using System.Diagnostics;

namespace OpenGA.Net;

public class OpenGARunner<T>
{
    internal int CurrentEpoch = 0;

    internal Stopwatch StopWatch = new();

    private int _maxNumberOfChromosomes;

    private int _minNumberOfChromosomes;

    private float _mutationRate = 0.2f;

    private readonly ReproductionSelectorConfiguration<T> _reproductionSelectorConfig = new();

    private readonly CrossoverStrategyRegistration<T> _crossoverStrategyRegistration = new();

    private readonly ReplacementStrategyRegistration<T> _replacementStrategyRegistration = new();

    private readonly TerminationStrategyConfiguration<T> _terminationStrategyConfig = new();

    private Chromosome<T>[] Population { get; set; } = [];

    internal double HighestFitness => Population.Max(c => c.Fitness);

    /// <summary>
    /// Gets the current state of the genetic algorithm including epoch, duration, and fitness metrics.
    /// </summary>
    internal GeneticAlgorithmState CurrentState => new(CurrentEpoch, StopWatch, HighestFitness);

    private readonly Random _random = new();

    private OpenGARunner() { }

    /// <summary>
    /// This method is used to initialize the GA Runner. The method expects a population, that is a collection
    /// of chromosomes where each chromosome represents a random solution to the problem at hand.
    /// </summary>
    /// <param name="initialPopulation">The initial population of chromosomes</param>
    /// <param name="minPopulationPercentage">Minimum population size as a percentage of initial population (0.0 to 1.0, default: 0.5 = 50%)</param>
    /// <param name="maxPopulationPercentage">Maximum population size as a percentage of initial population (1.0+, default: 2.0 = 200%)</param>
    /// <returns>A configured OpenGARunner instance</returns>
    /// <exception cref="MissingInitialPopulationException">Thrown when initial population is empty</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when percentage parameters are out of valid range</exception>
    public static OpenGARunner<T> Initialize(Chromosome<T>[] initialPopulation, float minPopulationPercentage = 0.5f, float maxPopulationPercentage = 2.0f)
    {
        if (initialPopulation is [])
        {
            throw new MissingInitialPopulationException("Initial population cannot be empty.");
        }

        if (minPopulationPercentage <= 0.0f || minPopulationPercentage > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minPopulationPercentage), "Minimum population percentage must be between 0.0 and 1.0.");
        }

        if (maxPopulationPercentage < 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPopulationPercentage), "Maximum population percentage must be 1.0 or greater.");
        }

        if (minPopulationPercentage >= maxPopulationPercentage)
        {
            throw new ArgumentOutOfRangeException(nameof(minPopulationPercentage), "Minimum population percentage must be less than maximum population percentage.");
        }

        var initialPopulationSize = initialPopulation.Length;
        var minPopulationSize = Math.Max(1, (int)(initialPopulationSize * minPopulationPercentage));
        var maxPopulationSize = (int)(initialPopulationSize * maxPopulationPercentage);

        return new OpenGARunner<T>
        {
            Population = initialPopulation,
            _minNumberOfChromosomes = minPopulationSize,
            _maxNumberOfChromosomes = maxPopulationSize
        };
    }

    /// <summary>
    /// The likelihood that a chromosome will mutate within an epoch/generation. Defaults to 0.2 (20%).
    /// </summary>
    /// <param name="mutationRate">Value should be between 0 and 1, where 0 indicates that no mutation will occur while 1 indicates that mutation will happen 100% of the time.</param>
    public OpenGARunner<T> MutationRate(float mutationRate)
    {
        if (mutationRate < 0 || mutationRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(mutationRate), "Value must be between 0 and 1.");
        }

        _mutationRate = mutationRate;
        return this;
    }

    public OpenGARunner<T> ApplyReproductionSelector(Action<ReproductionSelectorConfiguration<T>> selectorConfigurator)
    {
        ArgumentNullException.ThrowIfNull(selectorConfigurator, nameof(selectorConfigurator));

        selectorConfigurator(_reproductionSelectorConfig);

        return this;
    }

    public OpenGARunner<T> Crossover(Action<CrossoverStrategyRegistration<T>> crossoverStrategyRegistration)
    {
        crossoverStrategyRegistration(_crossoverStrategyRegistration);

        return this;
    }

    public OpenGARunner<T> Replacement(Action<ReplacementStrategyRegistration<T>> replacementStrategyRegistration)
    {
        ArgumentNullException.ThrowIfNull(replacementStrategyRegistration, nameof(replacementStrategyRegistration));

        replacementStrategyRegistration(_replacementStrategyRegistration);

        return this;
    }

    /// <summary>
    /// Configures termination strategies that determine when the genetic algorithm should stop.
    /// </summary>
    /// <param name="terminationStrategyConfigurators">One or more configuration actions for termination strategies.</param>
    /// <returns>The OpenGARunner instance for method chaining.</returns>
    /// <remarks>
    /// Termination strategies define the conditions under which the genetic algorithm should halt execution.
    /// Multiple strategies can be configured, and the algorithm will stop when any of them are met.
    /// 
    /// Available termination strategies include:
    /// - Maximum epochs: Stop after a specified number of generations
    /// - Maximum duration: Stop after a specified time period
    /// - Target standard deviation: Stop when population diversity drops below a threshold
    /// - Target fitness: Stop when a chromosome achieves the desired fitness value
    /// 
    /// Example usage:
    /// <code>
    /// .Termination(config => config.MaximumEpochs(100))
    /// .Termination(
    ///     config => config.MaximumEpochs(500),
    ///     config => config.MaximumDuration(TimeSpan.FromMinutes(5))
    /// )
    /// </code>
    /// </remarks>
    public OpenGARunner<T> Termination(params Action<TerminationStrategyConfiguration<T>>[] terminationStrategyConfigurators)
    {
        ArgumentNullException.ThrowIfNull(terminationStrategyConfigurators, nameof(terminationStrategyConfigurators));

        foreach (var terminationStrategyConfigurator in terminationStrategyConfigurators)
        {
            terminationStrategyConfigurator(_terminationStrategyConfig);
        }

        return this;
    }

    /// <summary>
    /// Applies default strategies for any components that haven't been explicitly configured.
    /// 
    /// This method ensures the genetic algorithm has all required components by applying
    /// sensible defaults when the user hasn't specified certain strategies. It also
    /// intelligently configures operator selection policies based on the number of
    /// crossover strategies available and whether custom weights are configured.
    /// 
    /// Operator selection policy precedence:
    /// 1. If explicitly configured policy exists → use it (validates compatibility with custom weights)
    /// 2. If any operator has custom weight > 0 → use CustomWeightPolicy
    /// 3. If single strategy → use FirstChoicePolicy
    /// 4. If multiple strategies → use AdaptivePursuitPolicy
    /// </summary>
    /// <exception cref="OperatorSelectionPolicyConflictException">
    /// Thrown when custom weights are configured but a non-CustomWeight operator selection policy is applied.
    /// </exception>
    private void DefaultMissingStrategies()
    {
        if (_reproductionSelectorConfig.ReproductionSelector is null)
        {
            _reproductionSelectorConfig.ApplyTournamentReproductionSelector();
        }

        if (_replacementStrategyRegistration.GetRegisteredReplacementStrategies() is [])
        {
            _replacementStrategyRegistration.RegisterSingle(s => s.Elitist());
        }

        if (_terminationStrategyConfig.TerminationStrategies is [])
        {
            _terminationStrategyConfig.MaximumEpochs(100);
        }

        _crossoverStrategyRegistration.ValidateAndDefault();

        // Setup replacement strategy operator selection policy
        if (_replacementStrategyRegistration.GetRegisteredReplacementStrategies() is { Count: 1 })
        {
            _replacementStrategyRegistration.WithPolicy(p => p.FirstChoice());
        }
        else
        {
            var hasCustomWeights = _replacementStrategyRegistration.GetRegisteredReplacementStrategies()
                .Any(strategy => strategy.CustomWeight > 0);

            if (_replacementStrategyRegistration.GetReplacementSelectionPolicy() is not null)
            {
                if (hasCustomWeights && _replacementStrategyRegistration.GetReplacementSelectionPolicy() is not CustomWeightPolicy)
                {
                    throw new OperatorSelectionPolicyConflictException(
                        @"Cannot apply a non-CustomWeight operator selection policy when replacement strategies 
                        have custom weights. Either remove the custom weights using WithCustomWeight(0) or use 
                        CustomWeights().");
                }
            }
            else if (hasCustomWeights)
            {
                // Auto-apply CustomWeightPolicy when weights are detected and no policy is explicitly set
                _replacementStrategyRegistration.WithPolicy(p => p.CustomWeights());
            }
            else
            {
                // If multiple replacement strategies and no operator policy specified then default to round robin
                _replacementStrategyRegistration.WithPolicy(p => p.RoundRobin());
            }
        }

        _replacementStrategyRegistration.GetReplacementSelectionPolicy()!
            .ApplyOperators([.._replacementStrategyRegistration.GetRegisteredReplacementStrategies()]);
    }

    /// <summary>
    /// Updates the Adaptive Pursuit Policy algorithm with performance feedback from a crossover operation.
    /// </summary>
    /// <param name="crossoverStrategy">The crossover strategy that was used</param>
    /// <param name="couple">The parent couple involved in crossover</param>
    /// <param name="offspring">The offspring produced by crossover</param>
    private void UpdateAdaptivePursuitReward(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseCrossoverStrategy<T> crossoverStrategy,
        Couple<T> couple,
        IEnumerable<Chromosome<T>> offspring)
    {
        var offspringList = offspring.ToList();
        if (offspringList.Count == 0)
        {
            return; // No offspring to evaluate
        }

        // Calculate best fitness among parents and offspring
        var bestParentFitness = Math.Max(couple.IndividualA.Fitness, couple.IndividualB.Fitness);
        var bestOffspringFitness = offspringList.Max(o => o.Fitness);

        // Calculate population fitness range for normalization
        var populationFitnesses = Population.Select(c => c.Fitness).ToArray();
        var populationFitnessRange = populationFitnesses.Max() - populationFitnesses.Min();

        // Calculate diversity among offspring (standard deviation of fitness)
        var offspringFitnesses = offspringList.Select(o => o.Fitness);
        var offspringDiversity = offspringFitnesses.StandardDeviation();

        // Update the reward for this crossover strategy
        adaptivePursuit.UpdateReward(
            crossoverStrategy,
            bestParentFitness,
            bestOffspringFitness,
            populationFitnessRange,
            offspringDiversity);
    }

    /// <summary>
    /// Executes the genetic algorithm until one of the configured termination conditions is met.
    /// 
    /// This method runs the complete genetic algorithm process, including:
    /// - Validating that all required strategies (reproduction selector, crossover, replacement) are configured
    /// - Applying a default termination strategy (100 epochs) if none is specified
    /// - Iteratively evolving the population through selection, crossover, mutation, and replacement
    /// - Tracking algorithm state including current epoch, duration, and fitness metrics
    /// - Terminating when any configured termination condition is satisfied
    /// 
    /// The algorithm follows these steps each generation:
    /// 1. Check termination conditions
    /// 2. Generate offspring through reproduction selection and crossover
    /// 3. Apply replacement strategy to create new population
    /// 4. Apply mutation and genetic repair to all chromosomes
    /// 5. Update chromosome ages and reset offspring ages
    /// </summary>
    /// <returns>
    /// The chromosome with the highest fitness value from the final population.
    /// This represents the best solution found by the genetic algorithm.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the calculated number of offspring is invalid (≤ 0 or > 2× population size).
    /// </exception>
    public Chromosome<T> RunToCompletion()
    {
        StopWatch.Start();
        DefaultMissingStrategies();

        for (; ; CurrentEpoch++)
        {
            if (_terminationStrategyConfig.ShouldTerminate(CurrentState))
            {
                break;
            }

            List<Chromosome<T>> offspring = [];

            var replacementStrategy = (BaseReplacementStrategy<T>)_replacementStrategyRegistration.GetReplacementSelectionPolicy().SelectOperator(_random, CurrentEpoch);

            var requiredNumberOfOffspring = CalculateOptimalOffspringCount(replacementStrategy);

            var maxCouplesPerBatch = Math.Max(requiredNumberOfOffspring, _maxNumberOfChromosomes);

            while (offspring.Count < requiredNumberOfOffspring)
            {
                var remainingOffspringNeeded = requiredNumberOfOffspring - offspring.Count;
                var couplesForThisBatch = Math.Min(maxCouplesPerBatch, remainingOffspringNeeded * 2); // Generate extra to account for failed crossovers
                var couples = _reproductionSelectorConfig.ReproductionSelector.SelectMatingPairs(Population, _random, couplesForThisBatch, CurrentEpoch);

                var offspringGeneratedInBatch = 0;

                foreach (var couple in couples)
                {
                    if (offspring.Count >= requiredNumberOfOffspring)
                    {
                        break;
                    }

                    var crossoverPolicy = _crossoverStrategyRegistration.GetCrossoverSelectionPolicy();

                    var crossoverStrategy = (BaseCrossoverStrategy<T>)crossoverPolicy.SelectOperator(_random, CurrentEpoch);

                    var crossoverRate = crossoverStrategy.CrossoverRateOverride ?? _crossoverStrategyRegistration.GetCrossoverRate();

                    if (_random.NextDouble() <= crossoverRate)
                    {
                        var newOffspring = crossoverStrategy.Crossover(couple, _random);

                        foreach (var child in newOffspring)
                        {
                            child.InvalidateFitness();
                        }

                        if (crossoverPolicy is AdaptivePursuitPolicy adaptivePursuit)
                        {
                            UpdateAdaptivePursuitReward(adaptivePursuit, crossoverStrategy, couple, newOffspring);
                        }

                        offspring.AddRange(newOffspring);
                        offspringGeneratedInBatch += newOffspring.Count();
                    }
                }

                if (offspringGeneratedInBatch == 0)
                {
                    break;
                }
            }

            Population = replacementStrategy.ApplyReplacement(Population, [.. offspring], _random, CurrentEpoch);

            foreach (var chromosome in Population)
            {
                if (_random.NextDouble() <= _mutationRate)
                {
                    chromosome.Mutate();
                    chromosome.InvalidateFitness();
                }

                chromosome.GeneticRepair();
                chromosome.InvalidateFitness();
                chromosome.IncrementAge();
            }

            foreach (var child in offspring)
            {
                child.ResetAge();
            }
        }

        StopWatch.Stop();

        return Population.OrderByDescending(c => c.Fitness).First();
    }

    private int CalculateOptimalOffspringCount(BaseReplacementStrategy<T> replacementStrategy)
    {
        int result;
        var currentPopulationSize = Population.Length;

        var customOverride = _replacementStrategyRegistration.GetOffspringGenerationRateOverride();
        if (customOverride.HasValue)
        {
            result = Math.Max(1, (int)(currentPopulationSize * customOverride.Value));
        }
        else
        {
            result = Math.Max(1, (int)(currentPopulationSize * replacementStrategy.RecommendedOffspringGenerationRate));
        }

        // Ensure the result respects population bounds
        // The offspring count should be sufficient to potentially reach max population
        var maxAllowableOffspring = _maxNumberOfChromosomes - _minNumberOfChromosomes;
        var minRequiredOffspring = Math.Max(1, _minNumberOfChromosomes - currentPopulationSize);

        // Clamp the result within reasonable bounds
        result = Math.Max(minRequiredOffspring, Math.Min(result, maxAllowableOffspring));

        if (result <= 0)
        {
            throw new InvalidOperationException("Required number of offspring must be greater than zero. Check replacement strategy configuration.");
        }

        if (result > _maxNumberOfChromosomes * 2)
        {
            throw new InvalidOperationException($"Required number of offspring ({result}) exceeds reasonable bounds (maximum: {_maxNumberOfChromosomes * 2}). This may indicate an issue with the replacement strategy configuration.");
        }

        return result;
    }
}