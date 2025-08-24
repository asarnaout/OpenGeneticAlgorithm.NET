using OpenGA.Net.Exceptions;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;
using OpenGA.Net.Termination;

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

    private float? _customOffspringPercentage = null;

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
    /// Sets a custom offspring percentage that overrides the automatic calculation based on replacement strategy.
    /// This allows fine-tuning of the genetic algorithm's behavior for specific use cases.
    /// </summary>
    /// <param name="offspringPercentage">The percentage of the population size to generate as offspring (0.0 to 2.0). 
    /// Values above 1.0 generate more offspring than the population size, which can increase selection pressure.</param>
    /// <returns>The OpenGARunner instance for method chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offspringPercentage is not between 0.0 and 2.0</exception>
    public OpenGARunner<T> OffspringPercentage(float offspringPercentage)
    {
        if (offspringPercentage <= 0.0f || offspringPercentage > 2.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(offspringPercentage), "Offspring percentage must be between 0.0 and 2.0.");
        }

        _customOffspringPercentage = offspringPercentage;
        return this;
    }

    public OpenGARunner<T> ApplyReproductionSelector(Action<ReproductionSelectorConfiguration<T>> selectorConfigurator)
    {
        ArgumentNullException.ThrowIfNull(selectorConfigurator, nameof(selectorConfigurator));

        selectorConfigurator(_reproductionSelectorConfig);

        return this;
    }

    public OpenGARunner<T> ApplyCrossoverStrategy(Action<CrossoverStrategyConfiguration<T>> crossoverStrategyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(crossoverStrategyConfigurator, nameof(crossoverStrategyConfigurator));

        crossoverStrategyConfigurator(_crossoverStrategyConfig);

        return this;
    }

    public OpenGARunner<T> ApplyReplacementStrategy(Action<ReplacementStrategyConfiguration<T>> replacementStrategyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(replacementStrategyConfigurator, nameof(replacementStrategyConfigurator));

        replacementStrategyConfigurator(_replacementStrategyConfig);

        return this;
    }
    
    public OpenGARunner<T> ApplyTerminationStrategy(Action<TerminationStrategyConfiguration<T>> terminationStrategyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(terminationStrategyConfigurator, nameof(terminationStrategyConfigurator));

        terminationStrategyConfigurator(_terminationStrategyConfig);

        return this;
    }

    /// <summary>
    /// Calculates the optimal number of offspring for the current replacement strategy.
    /// </summary>
    /// <returns>The recommended number of offspring to generate</returns>
    private int CalculateOptimalOffspringCount()
    {
        // If user specified a custom percentage, use that instead
        if (_customOffspringPercentage.HasValue)
        {
            return Math.Max(1, (int)(_maxNumberOfChromosomes * _customOffspringPercentage.Value));
        }

        return _replacementStrategyConfig.ReplacementStrategy switch
        {
            // Generational replacement: Replace entire population
            GenerationalReplacementStrategy<T> => _maxNumberOfChromosomes,
            
            // Elitist replacement: Replace all non-elite chromosomes
            ElitistReplacementStrategy<T> elitistStrat => _maxNumberOfChromosomes - (int)(_maxNumberOfChromosomes * elitistStrat.ElitePercentage),
            
            // Age-based replacement: Moderate turnover (older chromosomes more likely to be replaced)
            // Generally want to replace about 30-40% to maintain diversity while preserving some experience
            AgeBasedReplacementStrategy<T> => Math.Max(1, (int)(_maxNumberOfChromosomes * AgeBasedReplacementStrategy<T>.RecommendedOffspringPercentage)),
            
            // Tournament replacement: Moderate to high turnover depending on selection pressure
            // Tournament tends to be more selective, so 40-60% replacement works well
            TournamentReplacementStrategy<T> => Math.Max(1, (int)(_maxNumberOfChromosomes * TournamentReplacementStrategy<T>.RecommendedOffspringPercentage)),
            
            // Random elimination: Conservative approach since it's completely random
            // Lower percentage to avoid losing good solutions by chance
            RandomEliminationReplacementStrategy<T> => Math.Max(1, (int)(_maxNumberOfChromosomes * RandomEliminationReplacementStrategy<T>.RecommendedOffspringPercentage)),
            
            // Boltzmann replacement: Dynamic based on temperature, but generally moderate
            // Temperature controls selection pressure, so moderate replacement works well
            BoltzmannReplacementStrategy<T> => Math.Max(1, (int)(_maxNumberOfChromosomes * BoltzmannReplacementStrategy<T>.RecommendedOffspringPercentage)),
            
            // Default fallback for any custom strategies
            _ => Math.Max(1, (int)(_maxNumberOfChromosomes * BaseReplacementStrategy<T>.DefaultOffspringPercentage))
        };
    }

    public Chromosome<T> RunToCompletion()
    {
        if (_terminationStrategyConfig.TerminationStrategies is [])
        {
            // If no termination strategy is specified, default to max epochs
            _terminationStrategyConfig.ApplyMaximumEpochsTerminationStrategy(100);
        }

        if (_reproductionSelectorConfig.ReproductionSelector is null)
        {
            throw new MissingReproductionSelectorsException("No reproduction selector is specified. Consider calling OpenGARunner<T>.ApplyReproductionSelector(...) to specify a selector.");
        }

        if (_crossoverStrategyConfig.CrossoverStrategy is null)
        {
            throw new MissingCrossoverStrategyException("No crossover strategy has been specified. Consider calling OpenGARunner<T>.ApplyCrossoverStrategy(...) to specify a crossover strategy.");
        }

        if (_replacementStrategyConfig.ReplacementStrategy is null)
        {
            throw new MissingReplacementStrategyException("No replacement strategy has been specified. Consider calling OpenGARunner<T>.ApplyReplacementStrategy(...) to specify a replacement strategy.");
        }

        var startTime = DateTime.UtcNow;

        for (; ; CurrentEpoch++)
        {
            CurrentDuration = DateTime.UtcNow - startTime;

            if (_terminationStrategyConfig.ShouldTerminate(CurrentState))
            {
                break;
            }

            List<Chromosome<T>> offspring = [];

            var requiredNumberOfOffspring = CalculateOptimalOffspringCount();

            // Validate offspring requirements
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

                var requiredNumberOfCouples = _crossoverStrategyConfig.CrossoverStrategy switch
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
                        var newOffspring = _crossoverStrategyConfig.CrossoverStrategy.Crossover(couple, _random);
                        foreach (var child in newOffspring)
                        {
                            child.InvalidateFitness(); // Invalidate fitness for new offspring
                        }
                        offspring.AddRange(newOffspring);
                    }
                }

                if (noCouples)
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
                    chromosome.InvalidateFitness(); // Invalidate fitness after mutation
                }

                chromosome.GeneticRepair();
                chromosome.InvalidateFitness(); // Invalidate fitness after genetic repair
                chromosome.IncrementAge();
            }

            foreach (var child in offspring)
            {
                child.ResetAge();
            }
        }

        return Population.OrderByDescending(c => c.Fitness).First();
    }
}