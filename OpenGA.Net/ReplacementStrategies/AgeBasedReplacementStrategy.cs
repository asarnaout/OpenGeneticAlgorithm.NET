namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// An age-based replacement strategy that selects chromosomes for elimination based on their age.
/// Older chromosomes (those that have survived more generations) have a higher probability of being
/// eliminated using a weighted roulette wheel selection mechanism.
/// 
/// This strategy encourages population turnover while still giving some chance for older chromosomes
/// to survive, which can help maintain genetic diversity and prevent premature convergence.
/// 
/// The selection probability is proportional to the chromosome's age, meaning that a chromosome
/// with age 10 is twice as likely to be eliminated as a chromosome with age 5.
/// 
/// Example usage:
/// <code>
/// // Create an age-based replacement strategy
/// var replacementStrategy = new AgeBasedReplacementStrategy&lt;int&gt;();
/// 
/// // Apply replacement to create new population (older chromosomes more likely to be eliminated)
/// var newPopulation = replacementStrategy.ApplyReplacement(
///     currentPopulation, 
///     offspring, 
///     random);
/// </code>
/// </summary>
public class AgeBasedReplacementStrategy<T> : BaseReplacementStrategy<T>
{
    /// <summary>
    /// The recommended offspring generation rate for age-based replacement strategy.
    /// This moderate turnover rate (35%) maintains diversity while preserving some experienced chromosomes.
    /// </summary>
    internal const float RecommendedOffspringGenerationRate = 0.35f;
    /// <summary>
    /// Selects chromosomes from the population for elimination using age-weighted selection.
    /// Older chromosomes have a higher probability of being selected for elimination through
    /// a weighted roulette wheel where the weight is proportional to the chromosome's age.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="currentEpoch">The current epoch/generation number (not used in age-based elimination)</param>
    /// <returns>The chromosomes selected for elimination based on age weighting</returns>
    protected internal override IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random,
        int currentEpoch = 0)
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

        // Handle edge cases for age-based selection
        var maxAge = population.Max(c => c.Age);
        
        // If all chromosomes have age 0, fall back to random selection
        if (maxAge == 0)
        {
            var shuffledPopulation = population.OrderBy(x => random.Next()).ToArray();
            for (int i = 0; i < eliminationsNeeded; i++)
            {
                candidatesForElimination.Add(shuffledPopulation[i]);
            }
            return candidatesForElimination;
        }

        // Create a weighted roulette wheel where weight is proportional to age
        // Add 1 to age to ensure even age-0 chromosomes have some chance of selection
        var wheel = WeightedRouletteWheel<Chromosome<T>>.Init(
            [..population], 
            chromosome => chromosome.Age + 1.0);

        // Select chromosomes for elimination using the weighted roulette wheel
        for (int i = 0; i < eliminationsNeeded; i++)
        {
            var selectedChromosome = wheel.SpinAndReadjustWheel();
            candidatesForElimination.Add(selectedChromosome);
        }

        return candidatesForElimination;
    }
}
