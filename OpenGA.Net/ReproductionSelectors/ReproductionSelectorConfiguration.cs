namespace OpenGA.Net.ReproductionSelectors;

public class ReproductionSelectorConfiguration<T>
{
    internal BaseReproductionSelector<T> ReproductionSelector = default!;

    /// <summary>
    /// Parents are chosen at random regardless of their fitness.
    /// </summary>
    public BaseReproductionSelector<T> Random()
    {
        ReproductionSelector = new RandomReproductionSelector<T>();
        return ReproductionSelector;
    }

    /// <summary>
    /// The likelihood of an individual chromosome being chosen for mating is proportional to its fitness.
    /// </summary>
    public BaseReproductionSelector<T> FitnessWeightedRouletteWheel()
    {
        ReproductionSelector = new FitnessWeightedRouletteWheelReproductionSelector<T>();
        return ReproductionSelector;
    }

    /// <summary>
    /// Each iteration, n-individuals are chosen at random to form a tournament and out of this group, 2 individuals are chosen for mating.
    /// The number of tournaments held per iteration is stochastic and depends on the size of the population at each iteration.
    /// </summary>
    /// <param name="stochasticTournament">Defaults to true. If set to true, then the 2 individuals chosen for mating in each 
    /// tournament are the fittest 2 individuals in the tournament, otherwise a roulette wheel is spun to choose the two winners 
    /// out of the n-individuals, where the probability of winning is proportional to each individual's fitness.</param>
    public BaseReproductionSelector<T> Tournament(bool stochasticTournament = true)
    {
        ReproductionSelector = new TournamentReproductionSelector<T>(stochasticTournament);
        return ReproductionSelector;
    }

    /// <summary>
    /// Apply a custom strategy for choosing mating parents. Requires an instance of a subclass of <see cref="BaseReproductionSelector<T>">BaseReproductionSelector<T></see>
    /// to dictate which individuals will be chosen to take part in the crossover process.
    /// </summary>
    public BaseReproductionSelector<T> Custom(BaseReproductionSelector<T> reproductionSelector)
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
    public BaseReproductionSelector<T> RankSelection()
    {
        ReproductionSelector = new RankSelectionReproductionSelector<T>();
        return ReproductionSelector;
    }
}