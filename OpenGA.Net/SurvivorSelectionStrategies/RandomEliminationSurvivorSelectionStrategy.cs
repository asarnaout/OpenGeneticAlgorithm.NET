using OpenGA.Net.Extensions;

namespace OpenGA.Net.SurvivorSelectionStrategies;

/// <summary>
/// A random survivor selection strategy that eliminates chromosomes from the population to make room for offspring.
/// Each chromosome has an equal chance of being eliminated, but the total number of eliminations is controlled
/// to exactly match the number of offspring that need to be accommodated.
/// 
/// This strategy provides purely random selection of which chromosomes to eliminate, ensuring population
/// size stability while maintaining complete randomness in the elimination process.
/// 
/// Example usage:
/// <code>
/// // Create a survivor selection strategy for random elimination
/// var survivorSelectionStrategy = new RandomEliminationSurvivorSelectionStrategy&lt;int&gt;();
/// 
/// // Apply survivor selection to create new population (size will be maintained)
/// var newPopulation = survivorSelectionStrategy.ApplySurvivorSelection(
///     currentPopulation, 
///     offspring, 
///     random);
/// </code>
/// </summary>
public class RandomEliminationSurvivorSelectionStrategy<T> : BaseSurvivorSelectionStrategy<T>
{
    /// <summary>
    /// The recommended offspring generation rate for random elimination survivor selection strategy.
    /// This conservative turnover rate (25%) reduces the risk of losing good solutions through random elimination.
    /// </summary>
    internal override float RecommendedOffspringGenerationRate => 0.25f;
    
    /// <summary>
    /// Selects chromosomes from the population for elimination using random selection.
    /// Exactly the number of chromosomes needed to accommodate the offspring will be eliminated,
    /// but the selection of which chromosomes is completely random.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="currentEpoch">The current epoch/generation number (not used in random elimination)</param>
    /// <returns>The chromosomes selected for elimination through random selection</returns>
    protected internal override Task<IEnumerable<Chromosome<T>>> SelectChromosomesForEliminationAsync(
        Chromosome<T>[] population,
        Chromosome<T>[] offspring,
        Random random,
        int currentEpoch = 0)
    {
        if (population.Length == 0 || offspring.Length == 0)
        {
            return Task.FromResult(Enumerable.Empty<Chromosome<T>>());
        }

        // We need to eliminate as many chromosomes as we have offspring
        var eliminationsNeeded = Math.Min(offspring.Length, population.Length);

        // Optimization: if we need to eliminate the entire population, just return it directly
        if (eliminationsNeeded == population.Length)
        {
            return Task.FromResult<IEnumerable<Chromosome<T>>>(population);
        }

        var candidatesForElimination = new List<Chromosome<T>>(eliminationsNeeded);

        // Create a shuffled copy of the population for random selection using extension method
        var shuffledPopulation = population.FisherYatesShuffle(random);

        // Select exactly eliminationsNeeded chromosomes from the shuffled population
        for (int i = 0; i < eliminationsNeeded; i++)
        {
            candidatesForElimination.Add(shuffledPopulation[i]);
        }

        return Task.FromResult<IEnumerable<Chromosome<T>>>(candidatesForElimination);
    }
}
