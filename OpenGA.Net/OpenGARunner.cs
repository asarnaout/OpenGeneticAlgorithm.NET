using OpenGA.Net.Exceptions;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;
using OpenGA.Net.Termination;

namespace OpenGA.Net;

public class OpenGARunner<T>
{
    internal int MaxEpochs = 80;

    internal int CurrentEpoch = 0;

    internal TimeSpan CurrentDuration = TimeSpan.Zero;

    private int _maxNumberOfChromosomes;

    private float _mutationRate = 0.2f;

    private float _crossoverRate = 0.9f;

    private readonly ReproductionSelectorConfiguration<T> _reproductionSelectorConfig = new();

    private readonly CrossoverStrategyConfiguration<T> _crossoverStrategyConfig = new();

    private readonly ReplacementStrategyConfiguration<T> _replacementStrategyConfig = new();

    private readonly TerminationStrategyConfiguration<T> _terminationStrategyConfig = new();

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
    /// Specifies how many epochs/generations/iterations the genetic algorithm will run for. Defaults to 80.
    /// Higher values will allow the GA to find better results at a performance penalty and vice versa.
    /// </summary>
    public OpenGARunner<T> Epochs(int maxNumberOfEpochs)
    {
        if (maxNumberOfEpochs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxNumberOfEpochs), "Value must be greater than 0.");
        }

        MaxEpochs = maxNumberOfEpochs;
        _terminationStrategyConfig.ApplyMaximumEpochsTerminationStrategy();
        return this;
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

    public OpenGARunner<T> ApplyReproductionSelectors(params Action<ReproductionSelectorConfiguration<T>>[] selectorConfigurator)
    {
        ArgumentNullException.ThrowIfNull(selectorConfigurator, nameof(selectorConfigurator));

        foreach(var configurator in selectorConfigurator)
        {
            configurator(_reproductionSelectorConfig);
        }

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

    public Chromosome<T>[] RunToCompletion()
    {
        if (_reproductionSelectorConfig.ReproductionSelector is null)
        {
            throw new MissingReproductionSelectorsException("No reproduction selector is specified. Consider calling OpenGARunner<T>.ApplyReproductionSelectors(...) to specify a selector.");
        }

        if (_crossoverStrategyConfig.CrossoverStrategy is null)
        {
            throw new MissingCrossoverStrategyException("No crossover strategy has been specified. Consider calling OpenGARunner<T>.ApplyCrossoverStrategy(...) to specify a crossover strategy.");
        }

        if (_replacementStrategyConfig.ReplacementStrategy is null)
        {
            throw new MissingReplacementStrategyException("No replacement strategy has been specified. Consider calling OpenGARunner<T>.ApplyReplacementStrategy(...) to specify a replacement strategy.");
        }

        // Reset duration and start tracking time
        CurrentDuration = TimeSpan.Zero;
        var startTime = DateTime.UtcNow;

        for (; CurrentEpoch < MaxEpochs; CurrentEpoch++)
        {
            CurrentDuration = DateTime.UtcNow - startTime;
            
            if (_terminationStrategyConfig.ShouldTerminate(this))
            {
                break;
            }

            List<Chromosome<T>> offspring = [];

            var requiredNumberOfOffspring = _replacementStrategyConfig.ReplacementStrategy switch
            {
                GenerationalReplacementStrategy<T> => _maxNumberOfChromosomes,
                ElitistReplacementStrategy<T> elitistStrategy => (int)(_maxNumberOfChromosomes * elitistStrategy.ElitePercentage),
                _ => (int)(_maxNumberOfChromosomes * 0.5)
            };

            while (offspring.Count < requiredNumberOfOffspring)
            {
                var remainingOffspringNeeded = requiredNumberOfOffspring - offspring.Count;

                var requiredNumberOfCouples = _crossoverStrategyConfig.CrossoverStrategy switch
                {
                    OnePointCrossoverStrategy<T> => (int)Math.Ceiling(remainingOffspringNeeded / 2.0),
                    UniformCrossoverStrategy<T> => remainingOffspringNeeded,
                    _ => (int)Math.Ceiling(remainingOffspringNeeded / 2.0)
                };

                var couples = _reproductionSelectorConfig.ReproductionSelector.SelectMatingPairs(_population, _random, requiredNumberOfCouples);

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

            Population = _replacementStrategyConfig.ReplacementStrategy.ApplyReplacement(Population, [.. offspring], _random);

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

        return Population;
    }
}