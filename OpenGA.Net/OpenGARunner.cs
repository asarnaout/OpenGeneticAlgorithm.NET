using OpenGA.Net.Exceptions;
using OpenGA.Net.ParentSelectorStrategies;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.SurvivorSelectionStrategies;
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

    private readonly ParentSelectorRegistration<T> _parentSelectorRegistration = new();

    private readonly CrossoverStrategyRegistration<T> _crossoverStrategyRegistration = new();

    private readonly SurvivorSelectionStrategyRegistration<T> _survivorSelectionStrategyRegistration = new();

    private readonly TerminationStrategyConfiguration<T> _terminationStrategyConfig = new();

    internal Chromosome<T>[] Population { get; set; } = [];

    internal async Task<GeneticAlgorithmState> GetCurrentStateAsync()
    {
        var highestFitness = (await Task.WhenAll(Population.Select(x => x.GetCachedFitnessAsync()))).Max();
        return new(CurrentEpoch, StopWatch, highestFitness);
    }

    private Random _random = new();

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
    /// Sets a specific seed for the random number generator to enable deterministic behavior.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random number generator.</param>
    /// <returns>The OpenGARunner instance for method chaining.</returns>
    /// <remarks>
    /// This method enables reproducible results for testing, debugging, and research purposes.
    /// When a seed is set, the genetic algorithm will produce the same sequence of random decisions
    /// across multiple runs, making results deterministic and repeatable.
    /// 
    /// <b>Important:</b> Calling this method multiple times will reset the random state each time,
    /// discarding any previous random sequence progress. The random number generator will restart
    /// from the beginning of the sequence defined by the new seed.
    /// 
    /// This method should typically be called once during configuration, before starting the
    /// genetic algorithm execution with RunToCompletion().
    /// </remarks>
    public OpenGARunner<T> WithRandomSeed(int seed)
    {
        _random = new Random(seed);

        return this;
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
    /// Configures parent selection strategies that determine how mating pairs are chosen from the population.
    /// </summary>
    /// <param name="selectorConfigurator">A configuration action that sets up one or more parent selection strategies.</param>
    /// <returns>The OpenGARunner instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the selectorConfigurator is null.</exception>
    /// <remarks>
    /// Parent selection is a crucial component of genetic algorithms that determines which chromosomes
    /// from the current population are chosen as parents to produce offspring for the next generation.
    /// The quality of parent selection directly impacts the algorithm's balance between exploration
    /// and exploitation of the solution space.
    /// 
    /// This method supports both single and multiple parent selection strategies:
    /// 
    /// <b>Single Strategy Configuration:</b>
    /// Use RegisterSingle() when only one parent selection method is needed. If no strategies are
    /// configured, OpenGARunner automatically defaults to Tournament selection during execution.
    /// 
    /// <b>Multiple Strategy Configuration:</b>
    /// Use RegisterMulti() to configure multiple strategies with optional custom weights and
    /// operator selection policies. The framework intelligently applies defaults:
    /// - If custom weights are specified, CustomWeightPolicy is automatically applied
    /// - If no weights and no explicit policy, AdaptivePursuitPolicy is used by default
    /// - If explicit policy conflicts with custom weights, an exception is thrown
    /// 
    /// <b>Available Parent Selection Strategies:</b>
    /// - <b>Tournament:</b> Conducts tournaments among randomly selected chromosomes
    /// - <b>RouletteWheel:</b> Selects parents probabilistically based on fitness proportions
    /// - <b>Rank:</b> Uses fitness rankings instead of absolute values to prevent domination
    /// - <b>Random:</b> Selects parents randomly regardless of fitness
    /// - <b>Boltzmann:</b> Temperature-based selection with cooling schedules
    /// - <b>Custom:</b> User-defined selection strategies
    /// 
    /// The framework includes Adaptive Pursuit integration that learns which parent selectors
    /// perform best over time and adjusts selection probabilities accordingly, providing
    /// performance feedback through UpdateAdaptivePursuitRewardForParentSelection().
    /// </remarks>
    /// <example>
    /// <code>
    /// // Single strategy
    /// .ParentSelection(p => p.RegisterSingle(s => s.Tournament(tournamentSize: 5)))
    /// 
    /// // Multiple strategies with weights
    /// .ParentSelection(p => p.RegisterMulti(m => m
    ///     .Tournament(customWeight: 0.6f)
    ///     .RouletteWheel(customWeight: 0.4f)
    ///     .WithPolicy(policy => policy.AdaptivePursuit())))
    /// 
    /// // Advanced configuration
    /// .ParentSelection(p => p.RegisterMulti(m => m
    ///     .Tournament(stochasticTournament: true)
    ///     .Rank()
    ///     .Boltzmann(temperatureDecayRate: 0.05, initialTemperature: 1.0)
    ///     .WithPolicy(policy => policy.AdaptivePursuit(learningRate: 0.1))))
    /// </code>
    /// </example>
    public OpenGARunner<T> ParentSelection(Action<ParentSelectorRegistration<T>> selectorConfigurator)
    {
        ArgumentNullException.ThrowIfNull(selectorConfigurator, nameof(selectorConfigurator));

        selectorConfigurator(_parentSelectorRegistration);
        return this;
    }

    /// <summary>
    /// Configures crossover strategies that determine how genetic material is combined from parent chromosomes to create offspring.
    /// </summary>
    /// <param name="crossoverStrategyRegistration">A configuration action that sets up one or more crossover strategies.</param>
    /// <returns>The OpenGARunner instance for method chaining.</returns>
    /// <remarks>
    /// Crossover is the primary genetic operator responsible for combining genetic material from
    /// two parent chromosomes to produce offspring. This process is fundamental to genetic algorithms
    /// as it enables the exploration of new solution combinations while preserving beneficial
    /// traits from successful parents.
    /// 
    /// This method supports both single and multiple crossover strategy configurations:
    /// 
    /// <b>Single Strategy Configuration:</b>
    /// Use RegisterSingle() when only one crossover method is needed. If no strategies are
    /// configured, OpenGARunner automatically defaults to OnePointCrossover during execution.
    /// 
    /// <b>Multiple Strategy Configuration:</b>
    /// Use RegisterMulti() to configure multiple strategies with optional custom weights and
    /// operator selection policies. The framework intelligently applies defaults:
    /// - If custom weights are specified, CustomWeightPolicy is automatically applied
    /// - If no weights and no explicit policy, AdaptivePursuitPolicy is used by default
    /// - If explicit policy conflicts with custom weights, an exception is thrown
    /// 
    /// <b>Available Crossover Strategies:</b>
    /// - <b>OnePointCrossover:</b> Single crossover point divides parent chromosomes
    /// - <b>KPointCrossover:</b> Multiple crossover points for increased genetic mixing
    /// - <b>UniformCrossover:</b> Gene-by-gene random selection from parents
    /// - <b>Custom:</b> User-defined crossover strategies
    /// 
    /// <b>Crossover Rate Configuration:</b>
    /// Each crossover operation is subject to a crossover rate that determines the probability
    /// of crossover occurring. This can be configured globally using WithCrossoverRate() or
    /// overridden per strategy. The default rate is 0.9 (90%).
    /// 
    /// The framework includes Adaptive Pursuit integration that monitors crossover performance
    /// and adjusts strategy selection probabilities based on offspring quality metrics through
    /// UpdateAdaptivePursuitReward(), considering fitness improvement and genetic diversity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Single strategy with custom crossover rate
    /// .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()).WithCrossoverRate(0.8f))
    /// 
    /// // Multiple strategies with adaptive selection
    /// .Crossover(c => c.RegisterMulti(m => m
    ///     .OnePointCrossover()
    ///     .KPointCrossover(numberOfPoints: 2)
    ///     .UniformCrossover()
    ///     .WithPolicy(policy => policy.AdaptivePursuit()))
    ///     .WithCrossoverRate(0.9f))
    /// 
    /// // Weighted strategies
    /// .Crossover(c => c.RegisterMulti(m => m
    ///     .OnePointCrossover(customWeight: 0.5f)
    ///     .KPointCrossover(numberOfPoints: 3, customWeight: 0.3f)
    ///     .UniformCrossover(customWeight: 0.2f))
    ///     .WithCrossoverRate(0.85f))
    /// </code>
    /// </example>
    public OpenGARunner<T> Crossover(Action<CrossoverStrategyRegistration<T>> crossoverStrategyRegistration)
    {
        crossoverStrategyRegistration(_crossoverStrategyRegistration);

        return this;
    }

    /// <summary>
    /// Configures survivor selection strategies that determine which individuals survive to the next generation.
    /// </summary>
    /// <param name="survivorSelectionStrategyRegistration">A configuration action that sets up one or more survivor selection strategies.</param>
    /// <returns>The OpenGARunner instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the survivorSelectionStrategyRegistration is null.</exception>
    /// <remarks>
    /// Survivor selection (also known as replacement or environmental selection) is a critical
    /// genetic algorithm component that determines which chromosomes from the combined population
    /// of parents and offspring will survive to form the next generation. This process directly
    /// controls population dynamics, selection pressure, and the balance between maintaining
    /// genetic diversity and preserving high-fitness solutions.
    /// 
    /// This method supports both single and multiple survivor selection strategy configurations:
    /// 
    /// <b>Single Strategy Configuration:</b>
    /// Use RegisterSingle() when only one survivor selection method is needed. If no strategies
    /// are configured, OpenGARunner automatically defaults to ElitistSurvivorSelectionStrategy
    /// during execution.
    /// 
    /// <b>Multiple Strategy Configuration:</b>
    /// Use RegisterMulti() to configure multiple strategies with optional custom weights and
    /// operator selection policies. The framework intelligently applies defaults:
    /// - If custom weights are specified, CustomWeightPolicy is automatically applied
    /// - If no weights and no explicit policy, AdaptivePursuitPolicy is used by default
    /// - If explicit policy conflicts with custom weights, an exception is thrown
    /// 
    /// <b>Available Survivor Selection Strategies:</b>
    /// - <b>Elitist:</b> Protects top performers while replacing others with offspring
    /// - <b>Generational:</b> Completely replaces parent population with offspring
    /// - <b>Tournament:</b> Eliminates individuals through competitive tournaments
    /// - <b>Random:</b> Randomly eliminates individuals to make room for offspring
    /// - <b>AgeBased:</b> Eliminates older chromosomes to encourage population turnover
    /// - <b>Boltzmann:</b> Temperature-based elimination with cooling schedules
    /// - <b>Custom:</b> User-defined survival strategies
    /// 
    /// <b>Dynamic Population Sizing:</b>
    /// The framework supports dynamic population sizing within configured bounds (set during
    /// Initialize()). Survivor selection strategies work with CalculateOptimalOffspringCount()
    /// to determine appropriate offspring generation rates, which can be overridden using
    /// OverrideOffspringGenerationRate().
    /// 
    /// The framework includes Adaptive Pursuit integration that monitors survivor selection
    /// performance and adjusts strategy selection based on population fitness improvements
    /// and diversity metrics through UpdateAdaptivePursuitRewardForSurvivorSelection().
    /// </remarks>
    /// <example>
    /// <code>
    /// // Single strategy with elite preservation
    /// .SurvivorSelection(s => s.RegisterSingle(config => config.Elitist(elitePercentage: 0.1f)))
    /// 
    /// // Multiple strategies with custom offspring rate
    /// .SurvivorSelection(s => s.RegisterMulti(m => m
    ///     .Elitist(elitePercentage: 0.15f)
    ///     .Tournament(tournamentSize: 5)
    ///     .WithPolicy(policy => policy.AdaptivePursuit()))
    ///     .OverrideOffspringGenerationRate(0.8f))
    /// 
    /// // Advanced configuration with weights
    /// .SurvivorSelection(s => s.RegisterMulti(m => m
    ///     .Elitist(elitePercentage: 0.1f, customWeight: 0.6f)
    ///     .Tournament(tournamentSize: 3, stochasticTournament: true, customWeight: 0.3f)
    ///     .AgeBased(customWeight: 0.1f))
    ///     .OverrideOffspringGenerationRate(1.2f))
    /// </code>
    /// </example>
    public OpenGARunner<T> SurvivorSelection(Action<SurvivorSelectionStrategyRegistration<T>> survivorSelectionStrategyRegistration)
    {
        ArgumentNullException.ThrowIfNull(survivorSelectionStrategyRegistration, nameof(survivorSelectionStrategyRegistration));

        survivorSelectionStrategyRegistration(_survivorSelectionStrategyRegistration);

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
    /// .Termination(config => config.MaximumEpochs(100).MaximumDuration(TimeSpan.FromMinutes(5)))
    /// </code>
    /// </remarks>
    public OpenGARunner<T> Termination(Action<TerminationStrategyConfiguration<T>> terminationStrategyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(terminationStrategyConfigurator, nameof(terminationStrategyConfigurator));

        terminationStrategyConfigurator(_terminationStrategyConfig);

        return this;
    }

    private void DefaultMissingStrategies()
    {
        _parentSelectorRegistration.ValidateAndDefault(_random);

        if (_terminationStrategyConfig.TerminationStrategies is [])
        {
            _terminationStrategyConfig.MaximumEpochs(100);
        }

        _crossoverStrategyRegistration.ValidateAndDefault(_random);
        
        _survivorSelectionStrategyRegistration.ValidateAndDefault(_random);
    }

    private async Task UpdateAdaptivePursuitReward(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseOperator @operator,
        Couple<T> couple,
        IEnumerable<Chromosome<T>> offspring)
    {
        var offspringList = offspring.ToList();
        if (offspringList.Count == 0)
        {
            return; // No offspring to evaluate
        }

        // Calculate best fitness among parents and offspring
        var bestParentFitness = Math.Max(await couple.IndividualA.GetCachedFitnessAsync(), await couple.IndividualB.GetCachedFitnessAsync());
        var bestOffspringFitness = (await Task.WhenAll(offspringList.Select(o => o.GetCachedFitnessAsync()))).Max();

        // Calculate population fitness range for normalization
        var populationFitnesses = (await Task.WhenAll(Population.Select(c => c.GetCachedFitnessAsync()))).ToArray();
        var populationFitnessRange = populationFitnesses.Max() - populationFitnesses.Min();

        // Calculate diversity among offspring (standard deviation of fitness)
        var offspringFitnesses = await Task.WhenAll(offspringList.Select(o => o.GetCachedFitnessAsync()));
        var offspringDiversity = offspringFitnesses.StandardDeviation();

        // Update the reward for this crossover strategy
        adaptivePursuit.UpdateReward(
            @operator,
            bestParentFitness,
            bestOffspringFitness,
            populationFitnessRange,
            offspringDiversity);
    }

    /// <summary>
    /// Updates the Adaptive Pursuit Policy with performance feedback from a survivor selection operation.
    /// 
    /// Rationale:
    /// - Survivor selection quality is better captured by changes in the population as a whole, not only the single best individual.
    /// - We therefore use mean fitness improvement from pre- to post-survivor selection as the primary reward signal.
    /// - To encourage maintaining healthy exploration, we add a diversity component based on the change in fitness standard deviation.
    /// - Metrics are computed immediately after survivor selection and before mutation/repair to isolate the survivor selection effect.
    /// </summary>
    private static async Task UpdateAdaptivePursuitRewardForSurvivorSelection(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseSurvivorSelectionStrategy<T> survivorSelectionStrategy,
        Chromosome<T>[] preSurvivorSelectionPopulation,
        Chromosome<T>[] postSurvivorSelectionPopulation)
    {
        // Fitness arrays
        var preFitnesses = (await Task.WhenAll(preSurvivorSelectionPopulation.Select(c => c.GetCachedFitnessAsync()))).ToArray();
        var postFitnesses = (await Task.WhenAll(postSurvivorSelectionPopulation.Select(c => c.GetCachedFitnessAsync()))).ToArray();

        // Primary signal: mean fitness improvement across the whole population
        var preMean = preFitnesses.Average();
        var postMean = postFitnesses.Average();

        // Normalization scale: use the fitness range of the pre-survivor selection population to keep scale consistent
        var preRange = preFitnesses.Max() - preFitnesses.Min();

        // Diversity component: change in standard deviation (positive favors exploration)
        var preStd = preFitnesses.StandardDeviation();
        var postStd = postFitnesses.StandardDeviation();
        var diversityDelta = postStd - preStd;

        // Feed the signals to Adaptive Pursuit. We map mean improvement to the primary reward path,
        // and use diversity change as the auxiliary term (internally weighted by diversityWeight).
        adaptivePursuit.UpdateReward(
            survivorSelectionStrategy,
            preMean,
            postMean,
            preRange,
            diversityDelta);
    }

    /// <summary>
    /// Executes the genetic algorithm until one of the configured termination conditions is met.
    /// 
    /// This method runs the complete genetic algorithm process, including:
    /// - Validating that all required strategies (parent selection, crossover, survivor selection) are configured
    /// - Applying a default termination strategy (100 epochs) if none is specified
    /// - Iteratively evolving the population through selection, crossover, mutation, and survivor selection
    /// - Tracking algorithm state including current epoch, duration, and fitness metrics
    /// - Terminating when any configured termination condition is satisfied
    /// 
    /// The algorithm follows these steps each generation:
    /// 1. Check termination conditions
    /// 2. Generate offspring through reproduction selection and crossover
    /// 3. Apply survivor selection strategy to create new population
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
    public async Task<Chromosome<T>> RunToCompletionAsync()
    {
        StopWatch.Start();
        DefaultMissingStrategies();

        for (; ; CurrentEpoch++)
        {
            if (_terminationStrategyConfig.ShouldTerminate(await GetCurrentStateAsync()))
            {
                break;
            }

            List<Chromosome<T>> offspring = [];

            var survivorSelectionSelectionPolicy = _survivorSelectionStrategyRegistration.GetSurvivorSelectionSelectionPolicy();

            var survivorSelectionStrategy = (BaseSurvivorSelectionStrategy<T>)survivorSelectionSelectionPolicy.SelectOperator(_random, CurrentEpoch);

            var requiredNumberOfOffspring = CalculateOptimalOffspringCount(survivorSelectionStrategy);

            var maxCouplesPerBatch = Math.Max(requiredNumberOfOffspring, _maxNumberOfChromosomes);

            while (offspring.Count < requiredNumberOfOffspring)
            {
                var remainingOffspringNeeded = requiredNumberOfOffspring - offspring.Count;

                var couplesForThisBatch = Math.Min(maxCouplesPerBatch, remainingOffspringNeeded * 2); // Generate extra to account for failed crossovers
                
                var parentSelectorPolicy = _parentSelectorRegistration.GetParentSelectorSelectionPolicy();

                var parentSelector = (BaseParentSelectorStrategy<T>)parentSelectorPolicy.SelectOperator(_random, CurrentEpoch);

                var couples = await parentSelector.SelectMatingPairsAsync(Population, _random, couplesForThisBatch, CurrentEpoch);

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
                        var newOffspring = await crossoverStrategy.CrossoverAsync(couple, _random);

                        foreach (var child in newOffspring)
                        {
                            child.InvalidateFitness();
                        }

                        if (crossoverPolicy is AdaptivePursuitPolicy adaptivePursuit)
                        {
                            await UpdateAdaptivePursuitReward(adaptivePursuit, crossoverStrategy, couple, newOffspring);
                        }

                        if (parentSelectorPolicy is AdaptivePursuitPolicy parentAdaptivePursuit)
                        {
                            await UpdateAdaptivePursuitReward(parentAdaptivePursuit, parentSelector, couple, newOffspring);
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

            // Keep a snapshot to isolate survivor selection impact (copy array to avoid in-place modifications)
            var preSurvivorSelectionPopulation = Population.ToArray();

            Population = await survivorSelectionStrategy.ApplySurvivorSelectionAsync(Population, [.. offspring], _random, CurrentEpoch);

            // Update Adaptive Pursuit for survivor selection based on immediate post-survivor selection population (before mutation)
            if (survivorSelectionSelectionPolicy is AdaptivePursuitPolicy adaptiveSurvivorSelection)
            {
                await UpdateAdaptivePursuitRewardForSurvivorSelection(adaptiveSurvivorSelection, survivorSelectionStrategy, preSurvivorSelectionPopulation, Population);
            }

            foreach (var chromosome in Population)
            {
                if (_random.NextDouble() <= _mutationRate)
                {
                    await chromosome.MutateAsync();
                    chromosome.InvalidateFitness();
                }

                await chromosome.GeneticRepairAsync();
                chromosome.InvalidateFitness();
                chromosome.IncrementAge();
            }

            foreach (var child in offspring)
            {
                child.ResetAge();
            }
        }

        StopWatch.Stop();

        var fitnessValues = await Task.WhenAll(Population.Select(c => c.GetCachedFitnessAsync()));
        var bestIndex = 0;
        var bestFitness = fitnessValues[0];
        
        for (int i = 1; i < fitnessValues.Length; i++)
        {
            if (fitnessValues[i] > bestFitness)
            {
                bestFitness = fitnessValues[i];
                bestIndex = i;
            }
        }
        
        return Population[bestIndex];
    }

    private int CalculateOptimalOffspringCount(BaseSurvivorSelectionStrategy<T> survivorSelectionStrategy)
    {
        int result;
        var currentPopulationSize = Population.Length;

        var customOverride = _survivorSelectionStrategyRegistration.GetOffspringGenerationRateOverride();
        if (customOverride.HasValue)
        {
            result = Math.Max(1, (int)(currentPopulationSize * customOverride.Value));
        }
        else
        {
            result = Math.Max(1, (int)(currentPopulationSize * survivorSelectionStrategy.RecommendedOffspringGenerationRate));
        }

        // Ensure the result respects population bounds
        // The offspring count should be sufficient to potentially reach max population
        var maxAllowableOffspring = _maxNumberOfChromosomes - _minNumberOfChromosomes;
        var minRequiredOffspring = Math.Max(1, _minNumberOfChromosomes - currentPopulationSize);

        // Clamp the result within reasonable bounds
        result = Math.Max(minRequiredOffspring, Math.Min(result, maxAllowableOffspring));

        if (result <= 0)
        {
            throw new InvalidOperationException("Required number of offspring must be greater than zero. Check survivor selection strategy configuration.");
        }

        if (result > _maxNumberOfChromosomes * 2)
        {
            throw new InvalidOperationException($"Required number of offspring ({result}) exceeds reasonable bounds (maximum: {_maxNumberOfChromosomes * 2}). This may indicate an issue with the survivor selection strategy configuration.");
        }

        return result;
    }
}