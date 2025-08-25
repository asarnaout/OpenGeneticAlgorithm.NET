using OpenGA.Net.Exceptions;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;
using OpenGA.Net.Termination;
using OpenGA.Net.OperatorSelectionPolicies;
using OpenGA.Net.Extensions;

namespace OpenGA.Net;

public class OpenGARunner<T>
{
    internal int CurrentEpoch = 0;

    internal TimeSpan CurrentDuration = TimeSpan.Zero;

    private int _maxNumberOfChromosomes;

    private float _mutationRate = 0.2f;

    private float _crossoverRate = 0.9f;

    private readonly ReproductionSelectorConfiguration<T> _reproductionSelectorConfig = new();

    private readonly CrossoverStrategyConfiguration<T> _crossoverStrategyConfig = new();

    private readonly ReplacementStrategyConfiguration<T> _replacementStrategyConfig = new();

    private readonly TerminationStrategyConfiguration<T> _terminationStrategyConfig = new();

    private readonly OperatorSelectionPolicyConfiguration _crossoverSelectionPolicyConfig = new();

    private float? _customOffspringGenerationRate = null;

    private Chromosome<T>[] _population = [];

    private Chromosome<T>[] Population
    {
        get => _population;
        set
        {
            _population = value;
            _maxNumberOfChromosomes = _population.Length;
        }
    }

    internal double HighestFitness => Population.Max(c => c.Fitness);

    /// <summary>
    /// Gets the current state of the genetic algorithm including epoch, duration, and fitness metrics.
    /// </summary>
    internal GeneticAlgorithmState CurrentState => new(CurrentEpoch, CurrentDuration, HighestFitness);

    private readonly Random _random = new();

    private OpenGARunner() { }

    /// <summary>
    /// This method is used to initialize the GA Runner. The method expects a population, that is a collection
    /// of chromosomes where each chromosome represents a random solution to the problem at hand.
    /// </summary>
    public static OpenGARunner<T> Init(Chromosome<T>[] initialPopulation)
    {
        if (initialPopulation is [])
        {
            throw new MissingInitialPopulationException("Initial population cannot be empty.");
        }

        return new OpenGARunner<T>
        {
            Population = initialPopulation
        };
    }

    /// <summary>
    /// Specifies the maximum duration the genetic algorithm will run for before terminating.
    /// This provides an alternative termination condition to the maximum epochs.
    /// </summary>
    /// <param name="maximumDuration">The maximum time the algorithm should run before terminating.</param>
    public OpenGARunner<T> MaxDuration(TimeSpan maximumDuration)
    {
        _terminationStrategyConfig.ApplyMaximumDurationTerminationStrategy(maximumDuration);
        return this;
    }

    /// <summary>
    /// The maximum number of chromosomes that can ever exist in the population. Defaults to the initial number of chromosomes provided when creating the runner. 
    /// When the limit is reached, the specified <see cref="ReplacementStrategy">ReplacementStrategy</see> is used to keep the population within limits.
    /// </summary>
    public OpenGARunner<T> MaxPopulationSize(int maximumNumberOfChromosomes)
    {
        _maxNumberOfChromosomes = maximumNumberOfChromosomes;

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
    /// The crossover rate dictates the likelihood of 2 mating parents producing an offspring. Defaults to 0.9 (90%).
    /// </summary>
    /// <param name="crossoverRate">Value should be between 0 and 1, where 0 indicates no chance of success in reproduction while 1 indicates a 100% chance.</param>
    public OpenGARunner<T> CrossoverRate(float crossoverRate)
    {
        if (crossoverRate < 0 || crossoverRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(crossoverRate), "Value must be between 0 and 1.");
        }

        _crossoverRate = crossoverRate;
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

    public OpenGARunner<T> ApplyCrossoverStrategies(params Action<CrossoverStrategyConfiguration<T>>[] crossoverStrategyConfigurators)
    {
        ArgumentNullException.ThrowIfNull(crossoverStrategyConfigurators, nameof(crossoverStrategyConfigurators));

        foreach (var crossoverStrategyConfigurator in crossoverStrategyConfigurators)
        {
            crossoverStrategyConfigurator(_crossoverStrategyConfig);
        }

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
    /// Configures the operator selection policy for choosing between multiple crossover strategies.
    /// 
    /// When multiple crossover strategies are configured, an operator selection policy determines
    /// which strategy to use for each crossover operation. This enables adaptive behavior where
    /// the genetic algorithm can learn which operators work best for the specific problem.
    /// 
    /// If not explicitly configured:
    /// - Single crossover strategy: FirstChoicePolicy is automatically applied
    /// - Multiple crossover strategies: AdaptivePursuitPolicy is automatically applied
    /// </summary>
    /// <param name="policyConfigurator">Action to configure the operator selection policy</param>
    /// <returns>The OpenGARunner instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when policyConfigurator is null</exception>
    public OpenGARunner<T> ApplyOperatorSelectionPolicy(Action<OperatorSelectionPolicyConfiguration> policyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(policyConfigurator, nameof(policyConfigurator));

        policyConfigurator(_crossoverSelectionPolicyConfig);

        return this;
    }

    /// <summary>
    /// Applies default strategies for any components that haven't been explicitly configured.
    /// 
    /// This method ensures the genetic algorithm has all required components by applying
    /// sensible defaults when the user hasn't specified certain strategies. It also
    /// intelligently configures operator selection policies based on the number of
    /// crossover strategies available.
    /// </summary>
    private void DefaultMissingStrategies()
    {
        if (_reproductionSelectorConfig.ReproductionSelector is null)
        {
            _reproductionSelectorConfig.ApplyTournamentReproductionSelector();
        }

        if (_crossoverStrategyConfig.CrossoverStrategies is [])
        {
            _crossoverStrategyConfig.ApplyOnePointCrossoverStrategy();
        }

        if (_replacementStrategyConfig.ReplacementStrategy is null)
        {
            _replacementStrategyConfig.ApplyElitistReplacementStrategy();
        }

        if (_terminationStrategyConfig.TerminationStrategies is [])
        {
            _terminationStrategyConfig.ApplyMaximumEpochsTerminationStrategy(100);
        }

        if (_crossoverStrategyConfig.CrossoverStrategies is { Count: 1 })
        {
            _crossoverSelectionPolicyConfig.ApplyFirstChoicePolicy();
        }
        else if (_crossoverSelectionPolicyConfig.Policy is null) 
        {
            // If multiple crossover strategies and no operator policy specified then default to adaptive pursuit
            _crossoverSelectionPolicyConfig.ApplyAdaptivePursuitPolicy();
        }

        _crossoverSelectionPolicyConfig.Policy!.ApplyOperators([.._crossoverStrategyConfig.CrossoverStrategies]);
    }

    /// <summary>
    /// Updates the Adaptive Pursuit Policy algorithm with performance feedback from a crossover operation.
    /// </summary>
    /// <param name="crossoverStrategy">The crossover strategy that was used</param>
    /// <param name="parents">The parent chromosomes involved in crossover</param>
    /// <param name="offspring">The offspring produced by crossover</param>
    private void UpdateAdaptivePursuitReward(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseCrossoverStrategy<T> crossoverStrategy,
        HashSet<Chromosome<T>> parents,
        List<Chromosome<T>> offspring)
    {
        // Calculate best fitness among parents and offspring
        var bestParentFitness = parents.Max(p => p.Fitness);
        var bestOffspringFitness = offspring.Max(o => o.Fitness);

        // Calculate population fitness range for normalization
        var populationFitnesses = Population.Select(c => c.Fitness).ToArray();
        var populationFitnessRange = populationFitnesses.Max() - populationFitnesses.Min();

        // Calculate diversity among offspring (standard deviation of fitness)
        var offspringFitnesses = offspring.Select(o => o.Fitness);
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
        DefaultMissingStrategies();

        var startTime = DateTime.UtcNow;

        for (; ; CurrentEpoch++)
        {
            CurrentDuration = DateTime.UtcNow - startTime;

            if (_terminationStrategyConfig.ShouldTerminate(CurrentState))
            {
                break;
            }

            List<Chromosome<T>> offspring = [];

            // Track performance data for each strategy across the entire epoch
            var epochStrategyData = new Dictionary<BaseCrossoverStrategy<T>, (HashSet<Chromosome<T>> parents, List<Chromosome<T>> offspring)>();

            var requiredNumberOfOffspring = CalculateOptimalOffspringCount();

            if (requiredNumberOfOffspring <= 0)
            {
                throw new InvalidOperationException("Required number of offspring must be greater than zero. Check replacement strategy configuration.");
            }

            if (requiredNumberOfOffspring > _maxNumberOfChromosomes * 2)
            {
                throw new InvalidOperationException($"Required number of offspring ({requiredNumberOfOffspring}) exceeds reasonable bounds (maximum: {_maxNumberOfChromosomes * 2}). This may indicate an issue with the replacement strategy configuration.");
            }

            while (offspring.Count < requiredNumberOfOffspring)
            {
                var remainingOffspringNeeded = requiredNumberOfOffspring - offspring.Count;

                var crossoverStrategy = _crossoverStrategyConfig.CrossoverStrategies[0];

                if (_crossoverSelectionPolicyConfig.Policy is not null)
                {
                    crossoverStrategy = (BaseCrossoverStrategy<T>)_crossoverSelectionPolicyConfig.Policy.SelectOperator(_random);
                }

                var requiredNumberOfCouples = crossoverStrategy switch
                {
                    OnePointCrossoverStrategy<T> => (int)Math.Ceiling(remainingOffspringNeeded / 2.0),
                    KPointCrossoverStrategy<T> => (int)Math.Ceiling(remainingOffspringNeeded / 2.0),
                    UniformCrossoverStrategy<T> => remainingOffspringNeeded,
                    _ => (int)Math.Ceiling(remainingOffspringNeeded / 2.0)
                };

                var couples = _reproductionSelectorConfig.ReproductionSelector.SelectMatingPairs(_population, _random, requiredNumberOfCouples, CurrentEpoch);

                var noCouples = true;

                foreach (var couple in couples)
                {
                    noCouples = false;

                    if (offspring.Count >= requiredNumberOfOffspring)
                    {
                        break;
                    }

                    if (_random.NextDouble() <= _crossoverRate)
                    {
                        // Track parents and offspring for this strategy across the epoch
                        if (_crossoverSelectionPolicyConfig.Policy is AdaptivePursuitPolicy)
                        {
                            if (!epochStrategyData.TryGetValue(crossoverStrategy, out (HashSet<Chromosome<T>> parents, List<Chromosome<T>> offspring) value))
                            {
                                value = (new HashSet<Chromosome<T>>(), new List<Chromosome<T>>());
                                epochStrategyData[crossoverStrategy] = value;
                            }

                            value.parents.Add(couple.IndividualA);
                            value.parents.Add(couple.IndividualB);
                        }

                        var newOffspring = crossoverStrategy.Crossover(couple, _random);

                        foreach (var child in newOffspring)
                        {
                            child.InvalidateFitness(); // Invalidate fitness for new offspring
                        }

                        // Add offspring to strategy tracking
                        if (_crossoverSelectionPolicyConfig.Policy is AdaptivePursuitPolicy)
                        {
                            epochStrategyData[crossoverStrategy].offspring.AddRange(newOffspring);
                        }

                        offspring.AddRange(newOffspring);
                    }
                }

                if (noCouples)
                {
                    break;
                }
            }

            // Update Adaptive Pursuit with epoch-level performance data
            if (_crossoverSelectionPolicyConfig.Policy is AdaptivePursuitPolicy adaptivePursuit)
            {
                foreach (var (strategy, (parents, strategyOffspring)) in epochStrategyData)
                {
                    if (parents.Count > 0 && strategyOffspring.Count > 0)
                    {
                        UpdateAdaptivePursuitReward(adaptivePursuit, strategy, parents, strategyOffspring);
                    }
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

        return Population.OrderByDescending(c => c.Fitness).First();
    }
    
    private int CalculateOptimalOffspringCount()
    {
        if (_customOffspringGenerationRate.HasValue)
        {
            return Math.Max(1, (int)(_maxNumberOfChromosomes * _customOffspringGenerationRate.Value));
        }

        // Use the strategy's own recommended generation rate
        return Math.Max(1, (int)(_maxNumberOfChromosomes * _replacementStrategyConfig.ReplacementStrategy.RecommendedOffspringGenerationRate));
    }
}