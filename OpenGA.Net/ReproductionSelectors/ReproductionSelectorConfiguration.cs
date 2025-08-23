using OpenGA.Net.Exceptions;

namespace OpenGA.Net.ReproductionSelectors;

public class ReproductionSelectorConfiguration<T>
{
    internal BaseReproductionSelector<T> ReproductionSelector = default!;

    /// <summary>
    /// Parents are chosen at random regardless of their fitness.
    /// </summary>
    public BaseReproductionSelector<T> ApplyRandomReproductionSelector()
    {
        var result = new RandomReproductionSelector<T>();
        ReproductionSelector = result;
        return result;
    }

    /// <summary>
    /// The likelihood of an individual chromosome being chosen for mating is proportional to its fitness.
    /// </summary>
    public BaseReproductionSelector<T> ApplyFitnessWeightedRouletteWheelReproductionSelector()
    {
        var result = new FitnessWeightedRouletteWheelReproductionSelector<T>();
        ReproductionSelector = result;
        return result;
    }

    /// <summary>
    /// Each iteration, n-individuals are chosen at random to form a tournament and out of this group, 2 individuals are chosen for mating.
    /// The number of tournaments held per iteration is stochastic and depends on the size of the population at each iteration.
    /// </summary>
    /// <param name="stochasticTournament">Defaults to true. If set to true, then the 2 individuals chosen for mating in each 
    /// tournament are the fittest 2 individuals in the tournament, otherwise a roulette wheel is spun to choose the two winners 
    /// out of the n-individuals, where the probability of winning is proportional to each individual's fitness.</param>
    public BaseReproductionSelector<T> ApplyTournamentReproductionSelector(bool stochasticTournament = true)
    {
        var result = new TournamentReproductionSelector<T>(stochasticTournament);
        ReproductionSelector = result;
        return result;
    }

    /// <summary>
    /// Apply a custom strategy for choosing mating parents. Requires an instance of a subclass of <see cref="BaseReproductionSelector<T>">BaseReproductionSelector<T></see>
    /// to dictate which individuals will be chosen to take part in the crossover process.
    /// </summary>
    public BaseReproductionSelector<T> ApplyCustomReproductionSelector(BaseReproductionSelector<T> reproductionSelector)
    {
        ArgumentNullException.ThrowIfNull(reproductionSelector, nameof(reproductionSelector));
        ReproductionSelector = reproductionSelector;
        return reproductionSelector;
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
    public BaseReproductionSelector<T> ApplyRankSelectionReproductionSelector()
    {
        var result = new RankSelectionReproductionSelector<T>();
        ReproductionSelector = result;
        return result;
    }

    /// <summary>
    /// Boltzmann Selection reproduction selector that uses temperature-based selection probabilities with decay.
    /// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter
    /// that starts at 1.0 and decays over epochs using the specified decay rate.
    /// Higher temperature leads to more uniform selection (exploration), while lower temperature leads to more elitist selection (exploitation).
    /// The temperature decays linearly over epochs, providing a natural transition from exploration to exploitation.
    /// </summary>
    /// <param name="temperatureDecayRate">The rate at which temperature decays per epoch. 
    /// Higher values (e.g., 0.1) result in faster cooling and quicker transition to exploitation, 
    /// lower values (e.g., 0.01) result in slower cooling and prolonged exploration phase. 
    /// Must be greater than or equal to 0. Defaults to 0.01.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0.</exception>
    public BaseReproductionSelector<T> ApplyBoltzmannReproductionSelector(double temperatureDecayRate = 0.01)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        var result = new BoltzmannReproductionSelector<T>(temperatureDecayRate);
        ReproductionSelector = result;
        return result;
    }

    /// <summary>
    /// The fittest individual chromosomes are guaranteed to participate in the mating process in the current epoch/generation. Non elites may participate as well.
    /// </summary>
    /// <param name="proportionOfElitesInPopulation">The proportion of elites in the population. Example, if the rate is 0.2 and the population size is 100, then we have 20 elites who are guaranteed to take part in the mating process.</param>
    /// <param name="proportionOfNonElitesAllowedToMate">The proportion of non-elites allowed to take part in the mating process. Non elites are chosen randomly regardless of fitness.</param>
    /// <param name="allowMatingElitesWithNonElites">Defaults to true. Setting this value to false would restrict couples made up of an elite and non-elite members</param>
    public BaseReproductionSelector<T> ApplyElitistReproductionSelector(float proportionOfElitesInPopulation = 0.1f, float proportionOfNonElitesAllowedToMate = 0.01f, bool allowMatingElitesWithNonElites = true)
    {
        if (proportionOfElitesInPopulation <= 0 || proportionOfElitesInPopulation > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfElitesInPopulation), "Value must be greater than 0 and less than or equal to 1.");
        }

        if (proportionOfNonElitesAllowedToMate < 0 || proportionOfNonElitesAllowedToMate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfNonElitesAllowedToMate), "Value must be between 0 and 1.");
        }

        var result = new ElitistReproductionSelector<T>(allowMatingElitesWithNonElites, proportionOfElitesInPopulation, proportionOfNonElitesAllowedToMate);
        ReproductionSelector = result;
        return result;
    }
}