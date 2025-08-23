namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// Base class for all replacement strategies. Replacement strategies determine which chromosomes
/// from the current population should be eliminated to make room for new offspring.
/// </summary>
public abstract class BaseReplacementStrategy<T>
{
    /// <summary>
    /// Selects chromosomes from the population that should be eliminated to make room for offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <returns>The chromosomes that should be eliminated from the population</returns>
    protected internal abstract IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random);

    /// <summary>
    /// Applies the replacement strategy to create a new population by eliminating selected chromosomes
    /// and adding offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <returns>The new population after replacement</returns>
    public virtual Chromosome<T>[] ApplyReplacement(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random)
    {
        // Select chromosomes for elimination
        var chromosomesToEliminate = SelectChromosomesForElimination(population, offspring, random).ToHashSet();
        
        // Create new population excluding eliminated chromosomes and adding offspring
        var survivingPopulation = population.Where(c => !chromosomesToEliminate.Contains(c)).ToList();
        survivingPopulation.AddRange(offspring);

        return [..survivingPopulation];
    }
}
