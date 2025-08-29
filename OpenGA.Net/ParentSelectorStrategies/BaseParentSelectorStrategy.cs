namespace OpenGA.Net.ParentSelectorStrategies;

/// <summary>
/// Abstract base class for all parent selector strategies in the genetic algorithm.
/// Parent selector strategies determine which chromosomes from the population will be selected
/// as parents to participate in the crossover process to produce offspring.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public abstract class BaseParentSelectorStrategy<T> : BaseOperator
{
    /// <summary>
    /// Selects mating pairs from the population for crossover operations with epoch awareness.
    /// This virtual method provides a default implementation that delegates to the epoch-unaware version
    /// for strategies that don't need epoch information. Strategies that require epoch information
    /// (such as Boltzmann selection with temperature decay) should override this method.
    /// </summary>
    /// <param name="population">The current population of chromosomes to select parents from</param>
    /// <param name="random">Random number generator for stochastic selection operations</param>
    /// <param name="minimumNumberOfCouples">The minimum number of mating pairs that should be selected</param>
    /// <param name="currentEpoch">The current epoch/generation number, used for epoch-aware strategies like Boltzmann selection. Defaults to 0.</param>
    /// <returns>A collection of chromosome couples (mating pairs) selected for crossover</returns>
    protected internal virtual async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples, int currentEpoch = 0)
    {
        return await SelectMatingPairsAsync(population, random, minimumNumberOfCouples);
    }

    /// <summary>
    /// Abstract method that must be implemented by concrete parent selector strategies to define
    /// the core parent selection logic. This method performs the actual selection of mating pairs
    /// from the population based on the specific strategy's algorithm.
    /// </summary>
    /// <param name="population">The current population of chromosomes to select parents from</param>
    /// <param name="random">Random number generator for stochastic selection operations</param>
    /// <param name="minimumNumberOfCouples">The minimum number of mating pairs that should be selected</param>
    /// <returns>A collection of chromosome couples (mating pairs) selected for crossover</returns>
    protected internal abstract Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples);

    /// <summary>
    /// Creates mating pairs using stochastic selection with a weighted roulette wheel.
    /// This utility method is commonly used by fitness-proportionate selection strategies
    /// where chromosomes are selected based on their relative fitness or other weights.
    /// The roulette wheel is rebuilt for each selection to handle readjustment after each spin.
    /// </summary>
    /// <param name="candidates">The list of candidate chromosomes available for selection</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="minimumNumberOfCouples">The minimum number of couples to generate</param>
    /// <param name="rouletteWheelBuilder">A function that creates a weighted roulette wheel for selection.
    /// This function is called for each couple generation to ensure proper weight distribution.</param>
    /// <returns>An enumerable collection of chromosome couples created through stochastic selection</returns>
    /// <remarks>
    /// This method ensures that:
    /// - If there are 1 or fewer candidates, no couples are generated
    /// - Each couple is formed by spinning the roulette wheel twice
    /// - The wheel is readjusted after each spin to prevent selecting the same chromosome twice in a single couple
    /// </remarks>
    internal virtual IEnumerable<Couple<T>> CreateStochasticCouples(IList<Chromosome<T>> candidates, Random random, int minimumNumberOfCouples, Func<WeightedRouletteWheel<Chromosome<T>>> rouletteWheelBuilder)
    {
        if (candidates.Count <= 1)
        {
            yield break;
        }

        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            var rouletteWheel = rouletteWheelBuilder();

            var winner1 = rouletteWheel.SpinAndReadjustWheel();
            var winner2 = rouletteWheel.SpinAndReadjustWheel();

            yield return Couple<T>.Pair(winner1, winner2);
        }
    }

    /// <summary>
    /// Generates mating pairs from a population containing exactly two individuals.
    /// This utility method handles the edge case where the population size is exactly 2,
    /// allowing the genetic algorithm to continue operating even with minimal population diversity.
    /// The same two chromosomes are paired repeatedly to meet the required number of couples.
    /// </summary>
    /// <param name="candidates">The list of candidate chromosomes (must contain exactly 2 chromosomes)</param>
    /// <param name="minimumNumberOfCouples">The number of couples to generate from the two individuals</param>
    /// <returns>An enumerable collection of couples, each containing the same two chromosomes</returns>
    /// <remarks>
    /// This method is typically used as a fallback when:
    /// - Population size has been reduced to exactly 2 chromosomes
    /// - The genetic algorithm needs to continue with minimal diversity
    /// - Other selection strategies cannot operate effectively with such small populations
    /// 
    /// While this reduces genetic diversity, it allows the algorithm to potentially recover
    /// through mutation and prevents premature termination due to insufficient population size.
    /// </remarks>
    internal virtual IEnumerable<Couple<T>> GenerateCouplesFromATwoIndividualPopulation(IList<Chromosome<T>> candidates, int minimumNumberOfCouples)
    {
        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            yield return Couple<T>.Pair(candidates[0], candidates[1]);
        }
    }
}
