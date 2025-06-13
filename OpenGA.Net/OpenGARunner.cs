using OpenGA.Net.Exceptions;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.EliminationMechanisms;

namespace OpenGA.Net;

public class OpenGARunner<T>
{
    private int _epochs = 80;

    private int _maxNumberOfChromosomes;

    private float _mutationRate = 0.2f;

    private float _crossoverRate = 0.9f;

    private readonly ReproductionSelectorConfiguration<T> _reproductionSelectorConfig = new();

    private readonly CrossoverStrategyConfiguration<T> _crossoverStrategyConfig = new();

    private readonly EliminationMechanismConfiguration<T> _eliminationMechanismConfig = new();

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

    private readonly Random _random = new();

    private OpenGARunner() { }

    //TODO: CONVERGENCE!!!!!

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
    public OpenGARunner<T> Epochs(int numberOfEpochs)
    {
        if (numberOfEpochs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfEpochs), "Value must be greater than 0.");
        }

        _epochs = numberOfEpochs;
        return this;
    }

    /// <summary>
    /// The maximum number of chromosomes that can ever exist in the population. Defaults to the initial number of chromosomes provided when creating the runner. 
    /// When the limit is reached, the specified ReplacementStrategy is used to keep the population within limits.
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

    public OpenGARunner<T> ApplyEliminationMechanism(Action<EliminationMechanismConfiguration<T>> eliminationMechanismConfigurator)
    {
        ArgumentNullException.ThrowIfNull(eliminationMechanismConfigurator, nameof(eliminationMechanismConfigurator));

        eliminationMechanismConfigurator(_eliminationMechanismConfig);

        return this;
    }

    public void Start()
    {
        if (_reproductionSelectorConfig.ReproductionSelector is null)
        {
            throw new MissingReproductionSelectorsException("No reproduction selector has been specified. Consider calling OpenGARunner<T>.ApplyReproductionSelector(...) to specify a reproduction selector.");
        }

        if (_crossoverStrategyConfig.CrossoverStrategy is null)
        {
            throw new MissingCrossoverStrategyException("No crossover strategy has been specified. Consider calling OpenGARunner<T>.ApplyCrossoverStrategy(...) to specify a crossover strategy.");
        }

        for (var i = 0; i < _epochs; i++)
        {
            List<Couple<T>> couples = [];

            List<Chromosome<T>> offspring = [];

            //TODO:The value below should affect how many chromosomes will be replaced

            var requiredNumberOfOffspring = _random.Next(2, _maxNumberOfChromosomes);

            //TODO: Double check this: Tying the number of couples to the max number of chromosomes
            couples.AddRange(_reproductionSelectorConfig.ReproductionSelector.SelectMatingPairs(_population, _random, requiredNumberOfOffspring));

            while (offspring.Count < requiredNumberOfOffspring)
            {
                foreach (var couple in couples)
                {
                    if (offspring.Count >= requiredNumberOfOffspring)
                    {
                        break;
                    }

                    if (_random.NextDouble() > _crossoverRate)
                    {
                        offspring.AddRange(_crossoverStrategyConfig.CrossoverStrategy.Crossover(couple, _random));
                    }
                }
            }
            

            //TODO: Cant generate more children than the population limit, must double check this here.

            foreach (var chromosome in Population)
            {
                if (_random.NextDouble() > 1 - _mutationRate)
                {
                    chromosome.Mutate();
                }
            }
        }
    }
}