namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// A generational replacement strategy that completely replaces the entire parent population with offspring.
/// This strategy eliminates all parent chromosomes and replaces the entire population with the new generation
/// of offspring, implementing a classic generational genetic algorithm approach.
/// 
/// In this strategy, the population is completely renewed each generation, with no parent chromosomes
/// surviving to the next generation. This can help avoid premature convergence but may lose good
/// solutions if the offspring generation is not sufficiently large or diverse.
/// 
/// Example usage:
/// <code>
/// // Create a replacement strategy for generational replacement
/// var replacementStrategy = new GenerationalReplacementStrategy&lt;int&gt;();
/// 
/// // Apply replacement to create new population (entire population replaced)
/// var newPopulation = replacementStrategy.ApplyReplacement(
///     currentPopulation, 
///     offspring, 
///     random);
/// </code>
/// </summary>
public class GenerationalReplacementStrategy<T> : BaseReplacementStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random)
    {
        return population; // Note: This actually won't be called since the ApplyReplacement override will directly return the offspring.
    }

    /// <summary>
    /// Applies the generational replacement strategy by eliminating the entire parent population
    /// and replacing it completely with the offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator (not used in this strategy)</param>
    /// <returns>The new population consisting entirely of offspring</returns>
    public override Chromosome<T>[] ApplyReplacement(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random)
    {
        // In generational replacement, the new population consists entirely of offspring
        // No parent chromosomes survive to the next generation
        return [..offspring];
    }
}
