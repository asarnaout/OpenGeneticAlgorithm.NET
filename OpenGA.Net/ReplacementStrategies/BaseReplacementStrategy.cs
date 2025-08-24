namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// Base class for all replacement strategies. Replacement strategies determine which chromosomes
/// from the current population should be eliminated to make room for new offspring.
/// </summary>
public abstract class BaseReplacementStrategy<T>
{
    /// <summary>
    /// The recommended offspring generation rate for this specific replacement strategy.
    /// Each strategy defines its own optimal rate based on its selection characteristics.
    /// </summary>
    internal abstract float RecommendedOffspringGenerationRate { get; }
    
    /// <summary>
    /// Selects chromosomes from the population that should be eliminated to make room for offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="currentEpoch">The current epoch/generation number (defaults to 0 for non-epoch-aware strategies)</param>
    /// <returns>The chromosomes that should be eliminated from the population</returns>
    protected internal abstract IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population,
        Chromosome<T>[] offspring,
        Random random,
        int currentEpoch = 0);

    /// <summary>
    /// Applies the replacement strategy to create a new population by eliminating selected chromosomes
    /// and adding offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="currentEpoch">The current epoch/generation number (defaults to 0 for non-epoch-aware strategies)</param>
    /// <returns>The new population after replacement</returns>
    public virtual Chromosome<T>[] ApplyReplacement(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random,
        int currentEpoch = 0)
    {
        // Select chromosomes for elimination
        var chromosomesToEliminate = SelectChromosomesForElimination(population, offspring, random, currentEpoch);
        var eliminatedSet = chromosomesToEliminate.ToHashSet();
        
        // Create new population excluding eliminated chromosomes and adding offspring
        var survivingPopulation = population.Where(c => !eliminatedSet.Contains(c)).ToList();
        survivingPopulation.AddRange(offspring);

        return [..survivingPopulation];
    }
}
