using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net;

public class OpenGARunner<T>
{
    private int _epochs = 80;

    private int _maxNumberOfChromosomes;

    private double _mutationRate = 0.2;

    private double _crossoverRate = 0.9;

    private ReproductionSelectorConfiguration _reproductionSelectorConfiguration = new();

    private ReplacementConfiguration _replacementConfiguration = new();

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

    //TODO: Validate the collection is not empty
    public ICollection<BaseReproductionSelector<T>> _reproductionSelectorStrategiesToApply = [];

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
    public OpenGARunner<T> MutationRate(double mutationRate)
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
    public OpenGARunner<T> CrossoverRate(double crossoverRate)
    {
        if (crossoverRate < 0 || crossoverRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(crossoverRate), "Value must be between 0 and 1.");
        }

        _crossoverRate = crossoverRate;
        return this;
    }

    /// <summary>
    /// Parents are chosen at random regardless of their fitness.
    /// </summary>
    public OpenGARunner<T> ApplyRandomReproductionSelector()
    {
        _reproductionSelectorStrategiesToApply.Add(new RandomReproductionSelector<T>());

        return this;
    }

    /// <summary>
    /// The likelihood of an individual chromosome being chosen for mating is proportional to its fitness.
    /// </summary>
    public OpenGARunner<T> ApplyFitnessWeightedRouletteWheelReproductionSelector()
    {
        _reproductionSelectorStrategiesToApply.Add(new FitnessWeightedRouletteWheelReproductionSelector<T>());

        return this;
    }

    /// <summary>
    /// Each iteration, n-individuals are chosen at random to form a tournament and out of this group, 2 individuals are chosen for mating.
    /// The number of tournaments held per iteration is stochastic and depends on the size of the population at each iteration.
    /// </summary>
    /// <param name="stochasticTournament">Defaults to true. If set to true, then the 2 individuals chosen for mating in each 
    /// tournament are the fittest 2 individuals in the tournament, otherwise a roulette wheel is spun to choose the two winners 
    /// out of the n-individuals, where the probability of winning is proportional to each individual's fitness.</param>
    public OpenGARunner<T> ApplyTournamentReproductionSelector(bool stochasticTournament = true)
    {
        _reproductionSelectorStrategiesToApply.Add(new TournamentReproductionSelector<T>());

        _reproductionSelectorConfiguration.StochasticTournament = stochasticTournament;

        return this;
    }

    /// <summary>
    /// Apply a custom strategy for choosing mating parents. Requires an instance of a subclass of <see cref="BaseCrossoverSelector<T>">BaseCrossoverSelector<T></see>
    /// to dictate which individuals will be chosen to take part in the crossover process.
    /// </summary>
    public OpenGARunner<T> ApplyCustomReproductionSelector(BaseReproductionSelector<T> reproductionSelector)
    {
        _reproductionSelectorStrategiesToApply.Add(reproductionSelector);

        return this;
    }

    public OpenGARunner<T> ApplyBoltzmannReproductionSelector()
    {
        _reproductionSelectorStrategiesToApply.Add(new BoltzmannReproductionSelector<T>());

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
    public OpenGARunner<T> ApplyRankSelectionReproductionSelector()
    {
        _reproductionSelectorStrategiesToApply.Add(new RankSelectionReproductionSelector<T>());

        return this;
    }

    /// <summary>
    /// The fittest individual chromosomes are guaranteed to participate in the mating process in the current epoch/generation. Non elites may participate as well.
    /// </summary>
    /// <param name="proportionOfElitesInPopulation">The proportion of elites in the population. Example, if the rate is 0.2 and the population size is 100, then we have 20 elites who are guaranteed to take part in the mating process.</param>
    /// <param name="proportionOfNonElitesAllowedToMate">The proportion of non-elites allowed to take part in the mating process. Non elites are chosen randomly regardless of fitness.</param>
    /// <param name="allowMatingElitesWithNonElites">Defaults to true. Setting this value to false would restrict couples made up of an elite and non-elite members</param>
    public OpenGARunner<T> ApplyElitistReproductionSelector(double proportionOfElitesInPopulation = 0.1d, double proportionOfNonElitesAllowedToMate = 0.01d, bool allowMatingElitesWithNonElites = true)
    {
        if (proportionOfElitesInPopulation <= 0 || proportionOfElitesInPopulation > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfElitesInPopulation), "Value must be greater than 0 and less than or equal to 1.");
        }

        _reproductionSelectorConfiguration.ProportionOfElitesInPopulation = proportionOfElitesInPopulation;

        if (proportionOfNonElitesAllowedToMate < 0 || proportionOfNonElitesAllowedToMate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfNonElitesAllowedToMate), "Value must be between 0 and 1.");
        }

        _reproductionSelectorConfiguration.ProportionOfNonElitesAllowedToMate = proportionOfNonElitesAllowedToMate;

        _reproductionSelectorConfiguration.AllowMatingElitesWithNonElites = proportionOfNonElitesAllowedToMate > 0d && allowMatingElitesWithNonElites;

        _reproductionSelectorStrategiesToApply.Add(new ElitistReproductionSelector<T>());

        return this;
    }

    public async Task StartAsync()
    {
        for (var i = 0; i < _epochs; i++)
        {
            List<Couple<T>> couples = [];

            if (_reproductionSelectorStrategiesToApply.Count == 0)
            {
                //TODO: What to do here?
            }

            foreach (var crossoverSelectorStrategy in _reproductionSelectorStrategiesToApply)
            {
                var minimumNumberOfCouples = 0; //TODO: MUST ADJUST THIS
                //TODO: Must adjust tournament params as well

                couples.AddRange(crossoverSelectorStrategy.SelectMatingPairs(_population, _reproductionSelectorConfiguration, _random, minimumNumberOfCouples));
            }

            foreach (var chromosome in Population)
            {
                if (_random.NextDouble() > 1 - _mutationRate)
                {
                    await chromosome.MutateAsync();
                }
            }
        }
    }
}