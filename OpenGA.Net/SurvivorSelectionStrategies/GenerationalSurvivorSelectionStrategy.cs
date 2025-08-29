namespace OpenGA.Net.SurvivorSelectionStrategies;

/// <summary>
/// A generational survivor selection strategy that completely replaces the entire parent population with offspring.
/// This strategy eliminates all parent chromosomes and replaces the entire population with the new generation
/// of offspring, implementing a classic generational genetic algorithm approach.
/// 
/// In this strategy, the population is completely renewed each generation, with no parent chromosomes
/// surviving to the next generation. This can help avoid premature convergence but may lose good
/// solutions if the offspring generation is not sufficiently large or diverse.
/// 
/// Example usage:
/// <code>
/// // Create a survivor selection strategy for generational replacement
/// var survivorSelectionStrategy = new GenerationalSurvivorSelectionStrategy&lt;int&gt;();
/// 
/// // Apply survivor selection to create new population (entire population replaced)
/// var newPopulation = survivorSelectionStrategy.ApplySurvivorSelection(
///     currentPopulation, 
///     offspring, 
///     random);
/// </code>
/// </summary>
public class GenerationalSurvivorSelectionStrategy<T> : BaseSurvivorSelectionStrategy<T>
{
    /// <summary>
    /// The recommended offspring generation rate for generational survivor selection strategy.
    /// This strategy requires 100% replacement (1.0) to completely replace the entire population.
    /// </summary>
    internal override float RecommendedOffspringGenerationRate => 1.0f;
    
    /// <summary>
    /// Selects all chromosomes from the population for elimination, implementing a complete
    /// generational replacement where the entire parent population is replaced by offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator (not used in this strategy)</param>
    /// <param name="currentEpoch">The current epoch/generation number (not used in generational survivor selection)</param>
    /// <returns>All chromosomes from the current population for elimination</returns>
    protected internal override IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population,
        Chromosome<T>[] offspring,
        Random random,
        int currentEpoch = 0)
    {
        // In generational survivor selection, we eliminate the entire parent population
        return population;
    }
}
