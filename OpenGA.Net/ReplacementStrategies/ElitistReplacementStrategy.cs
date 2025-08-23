using OpenGA.Net.Extensions;

namespace OpenGA.Net.ReplacementStrategies;

/// <summary>
/// An elitist replacement strategy that guarantees survival for the top-performing chromosomes
/// based on their fitness values. This strategy protects a specified percentage of the best
/// chromosomes from elimination, ensuring that the fittest individuals survive to the next generation.
/// 
/// The elitist approach helps preserve good solutions while still allowing for population turnover
/// and the introduction of new genetic material through offspring. This can help prevent the loss
/// of high-quality solutions while maintaining genetic diversity.
/// 
/// Example usage:
/// <code>
/// // Create an elitist replacement strategy protecting top 15% of population
/// var replacementStrategy = new ElitistReplacementStrategy&lt;int&gt;(0.15);
/// 
/// // Apply replacement to create new population (elites protected)
/// var newPopulation = replacementStrategy.ApplyReplacement(
///     currentPopulation, 
///     offspring, 
///     random);
/// </code>
/// </summary>
public class ElitistReplacementStrategy<T> (float elitePercentage = 0.1f): BaseReplacementStrategy<T>
{
    private readonly float _elitePercentage = elitePercentage;

    /// <summary>
    /// Selects chromosomes for elimination while protecting elite chromosomes based on fitness.
    /// The top-performing chromosomes (based on fitness) are guaranteed survival, while the
    /// remaining chromosomes are eligible for elimination to make room for offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <returns>The non-elite chromosomes selected for elimination</returns>
    protected internal override IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random)
    {
        if (population.Length == 0 || offspring.Length == 0)
        {
            return [];
        }

        // Calculate how many elites to protect
        var eliteCount = (int)Math.Ceiling(population.Length * _elitePercentage);
        eliteCount = Math.Min(eliteCount, population.Length); // Ensure we don't exceed population size

        // Calculate fitness for all chromosomes and sort by fitness (descending - best first)
        var chromosomesWithFitness = population
            .Select(chromosome => new { Chromosome = chromosome, Fitness = chromosome.CalculateFitness() })
            .OrderByDescending(x => x.Fitness)
            .ToArray();

        // Identify elite chromosomes (top performers)
        var eliteChromosomes = chromosomesWithFitness
            .Take(eliteCount)
            .Select(x => x.Chromosome)
            .ToHashSet();

        // Get non-elite chromosomes that are eligible for elimination
        var eligibleForElimination = population.Where(c => !eliteChromosomes.Contains(c)).ToArray();

        // Determine how many chromosomes we need to eliminate
        var eliminationsNeeded = Math.Min(offspring.Length, eligibleForElimination.Length);

        // If we need to eliminate more than available non-elites, we can only eliminate what's available
        if (eliminationsNeeded <= 0)
        {
            return [];
        }

        // Select chromosomes for elimination from the non-elite pool
        // Use random selection among non-elites (could be enhanced with fitness-based selection)
        var shuffledEligible = eligibleForElimination.FisherYatesShuffled(random);

        return shuffledEligible.Take(eliminationsNeeded);
    }

    /// <summary>
    /// Gets the percentage of the population protected as elites.
    /// </summary>
    public float ElitePercentage => _elitePercentage;
}
