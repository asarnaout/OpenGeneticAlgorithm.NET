namespace OpenGA.Net.ReproductionSelectors;

/// <summary>
/// A random reproduction selector that chooses mating pairs completely at random, 
/// without considering fitness values. This strategy provides maximum genetic diversity 
/// but may not preserve beneficial traits as effectively as fitness-based selectors.
/// </summary>
/// <typeparam name="T">The type of the gene data contained in chromosomes</typeparam>
/// <remarks>
/// This selector implements a pure random mating strategy where:
/// - Every chromosome has an equal probability of being selected for mating
/// - Fitness values are completely ignored during selection
/// - Selection is done using a uniform weighted roulette wheel
/// - This can help maintain genetic diversity and avoid premature convergence
/// - Useful for exploring the search space without bias toward current best solutions
/// </remarks>
public class RandomReproductionSelector<T> : BaseReproductionSelector<T>
{
    /// <summary>
    /// Selects mating pairs from the population using pure random selection.
    /// Each chromosome has an equal probability of being chosen regardless of fitness.
    /// </summary>
    /// <param name="population">The array of chromosomes available for mating</param>
    /// <param name="random">Random number generator for stochastic selection</param>
    /// <param name="minimumNumberOfCouples">The minimum number of couples to generate</param>
    /// <returns>A collection of couples selected for mating</returns>
    /// <exception cref="ArgumentNullException">Thrown when population or random is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minimumNumberOfCouples is negative</exception>
    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        // Validate input parameters
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumNumberOfCouples);

        // Handle edge case: insufficient population for mating
        if (population.Length <= 1)
        {
            return [];
        }

        // Handle special case: exactly two individuals
        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        // Use stochastic coupling with uniform weights (random selection)
        // CreateStochasticCouples will:
        // 1. Shuffle the population randomly
        // 2. Create a uniform weighted roulette wheel (all chromosomes have equal probability)
        // 3. Spin the wheel twice for each couple, removing selected chromosomes to avoid duplicates
        // 4. Generate the requested number of couples
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(population));
    }
}
