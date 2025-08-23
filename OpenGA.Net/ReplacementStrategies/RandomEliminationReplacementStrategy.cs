namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// A random replacement strategy that eliminates chromosomes from the population to make room for offspring.
/// Each chromosome has an equal chance of being eliminated, but the total number of eliminations is controlled
/// to exactly match the number of offspring that need to be accommodated.
/// 
/// This strategy provides purely random selection of which chromosomes to eliminate, ensuring population
/// size stability while maintaining complete randomness in the elimination process.
/// 
/// Example usage:
/// <code>
/// // Create a replacement strategy for random elimination
/// var replacementStrategy = new RandomEliminationReplacementStrategy&lt;int&gt;();
/// 
/// // Apply replacement to create new population (size will be maintained)
/// var newPopulation = replacementStrategy.ApplyReplacement(
///     currentPopulation, 
///     offspring, 
///     random);
/// </code>
/// </summary>
public class RandomEliminationReplacementStrategy<T> : BaseReplacementStrategy<T>
{
    /// <summary>
    /// Initializes a new instance of the RandomEliminationReplacementStrategy.
    /// </summary>
    public RandomEliminationReplacementStrategy()
    {
    }

    /// <summary>
    /// Selects chromosomes from the population for elimination using random selection.
    /// Exactly the number of chromosomes needed to accommodate the offspring will be eliminated,
    /// but the selection of which chromosomes is completely random.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <returns>The chromosomes selected for elimination through random selection</returns>
    protected internal override IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random)
    {
        if (population.Length == 0 || offspring.Length == 0)
        {
            return [];
        }

        // We need to eliminate as many chromosomes as we have offspring
        var eliminationsNeeded = Math.Min(offspring.Length, population.Length);

        // Optimization: if we need to eliminate the entire population, just return it directly
        if (eliminationsNeeded == population.Length)
        {
            return population;
        }

        var candidatesForElimination = new List<Chromosome<T>>(eliminationsNeeded);

        // Create a shuffled copy of the population for random selection
        var shuffledPopulation = new Chromosome<T>[population.Length];
        Array.Copy(population, shuffledPopulation, population.Length);
        
        // Fisher-Yates shuffle for truly random ordering
        for (int i = shuffledPopulation.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffledPopulation[i], shuffledPopulation[j]) = (shuffledPopulation[j], shuffledPopulation[i]);
        }

        // Select exactly eliminationsNeeded chromosomes from the shuffled population
        for (int i = 0; i < eliminationsNeeded; i++)
        {
            candidatesForElimination.Add(shuffledPopulation[i]);
        }

        return candidatesForElimination;
    }

    /// <summary>
    /// Applies the random replacement strategy by eliminating exactly the number of chromosomes
    /// needed to accommodate the offspring, selected completely at random.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <returns>The new population with maintained size</returns>
    public override Chromosome<T>[] ApplyReplacement(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random)
    {
        // Select chromosomes for elimination based purely on probability
        var chromosomesToEliminate = SelectChromosomesForElimination(population, offspring, random).ToHashSet();
        
        // Create new population excluding eliminated chromosomes and adding all offspring
        var survivingPopulation = population.Where(c => !chromosomesToEliminate.Contains(c)).ToList();
        survivingPopulation.AddRange(offspring);

        // Return the population with maintained size
        return [..survivingPopulation];
    }
}
