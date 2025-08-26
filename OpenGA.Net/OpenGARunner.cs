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

    private readonly ReplacementStrategyConfiguration<T> _replacementStrategyConfig = new();

    private readonly TerminationStrategyConfiguration<T> _terminationStrategyConfig = new();

    private float? _customOffspringGenerationRate = null;

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

    /// <summary>
    /// Sets a custom offspring generation rate that overrides the automatic rate that is based on the chosen replacement strategy.
    /// This allows fine-tuning of the genetic algorithm's behavior for specific use cases by controlling
    /// how many offspring are generated relative to the population size.
    /// 
    /// For example, 0.5 generates offspring equal to 50% of population size, while 1.5 generates 150% of population size.
    /// Values above 1.0 generate more offspring than the population size, which can increase selection pressure.
    /// </summary>
    /// <param name="generationRate">The rate of offspring generation relative to population size (0.0 to 2.0). </param>
    /// <returns>The OpenGARunner instance for method chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when generationRate is not between 0.0 and 2.0</exception>
    public OpenGARunner<T> OverrideOffspringGenerationRate(float generationRate)
    {
        if (generationRate <= 0.0f || generationRate > 2.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(generationRate), "Offspring generation rate must be between 0.0 and 2.0.");
        }

        _customOffspringGenerationRate = generationRate;
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

    public OpenGARunner<T> ApplyReplacementStrategy(Action<ReplacementStrategyConfiguration<T>> replacementStrategyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(replacementStrategyConfigurator, nameof(replacementStrategyConfigurator));

        replacementStrategyConfigurator(_replacementStrategyConfig);

        return this;
    }

    public OpenGARunner<T> ApplyTerminationStrategies(params Action<TerminationStrategyConfiguration<T>>[] terminationStrategyConfigurators)
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

        if (_crossoverStrategyRegistration.GetRegisteredCrossoverStrategies() is [])
        {
            _crossoverStrategyRegistration.RegisterSingle(s => s.OnePointCrossover());
        }

        if (_replacementStrategyConfig.ReplacementStrategy is null)
        {
            _replacementStrategyConfig.ApplyElitistReplacementStrategy();
        }

        if (_terminationStrategyConfig.TerminationStrategies is [])
        {
            _terminationStrategyConfig.ApplyMaximumEpochsTerminationStrategy(100);
        }

        if (_crossoverStrategyRegistration.GetRegisteredCrossoverStrategies() is { Count: 1 })
        {
            _crossoverStrategyRegistration.WithPolicy(p => p.ApplyFirstChoicePolicy());
        }
        else
        {
            var hasCustomWeights = _crossoverStrategyRegistration.GetRegisteredCrossoverStrategies()
                .Any(strategy => strategy.CustomWeight > 0);

            if (_crossoverStrategyRegistration.GetCrossoverSelectionPolicy() is not null)
            {
                if (hasCustomWeights && _crossoverStrategyRegistration.GetCrossoverSelectionPolicy() is not CustomWeightPolicy)
                {
                    throw new OperatorSelectionPolicyConflictException(
                        @"Cannot apply a non-CustomWeight operator selection policy when crossover strategies 
                        have custom weights. Either remove the custom weights using WithCustomWeight(0) or use 
                        ApplyCustomWeightPolicy().");
                }
            }
            else if (hasCustomWeights)
            {
                // Auto-apply CustomWeightPolicy when weights are detected and no policy is explicitly set
                _crossoverStrategyRegistration.WithPolicy(p => p.ApplyCustomWeightPolicy());
            }
            else
            {
                // If multiple crossover strategies and no operator policy specified then default to adaptive pursuit
                _crossoverStrategyRegistration.WithPolicy(p => p.ApplyAdaptivePursuitPolicy());
            }
        }

        _crossoverStrategyRegistration.GetCrossoverSelectionPolicy()!
            .ApplyOperators([.._crossoverStrategyRegistration.GetRegisteredCrossoverStrategies()]);
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

            var requiredNumberOfOffspring = CalculateOptimalOffspringCount();

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

                    if (_random.NextDouble() <= _crossoverStrategyRegistration.GetCrossoverRate())
                    {
                        var crossoverStrategy = (BaseCrossoverStrategy<T>)_crossoverStrategyRegistration.GetCrossoverSelectionPolicy().SelectOperator(_random, CurrentEpoch);

                        var newOffspring = crossoverStrategy.Crossover(couple, _random);

                        foreach (var child in newOffspring)
                        {
                            child.InvalidateFitness();
                        }

                        if (_crossoverStrategyRegistration.GetCrossoverSelectionPolicy() is AdaptivePursuitPolicy adaptivePursuit)
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

            Population = _replacementStrategyConfig.ReplacementStrategy.ApplyReplacement(Population, [.. offspring], _random, CurrentEpoch);

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

    private int CalculateOptimalOffspringCount()
    {
        int result;
        var currentPopulationSize = Population.Length;

        if (_customOffspringGenerationRate.HasValue)
        {
            result = Math.Max(1, (int)(currentPopulationSize * _customOffspringGenerationRate.Value));
        }
        else
        {
            result = Math.Max(1, (int)(currentPopulationSize * _replacementStrategyConfig.ReplacementStrategy.RecommendedOffspringGenerationRate));
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