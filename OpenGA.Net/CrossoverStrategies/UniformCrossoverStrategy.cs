
using OpenGA.Net.Exceptions;

namespace OpenGA.Net.CrossoverStrategies;

/// <summary>
/// Implements uniform crossover genetic operator for chromosomes.
/// 
/// Uniform crossover works by independently selecting each gene from either parent with equal probability (50%).
/// This creates genetic diversity while maintaining the characteristics of both parents.
/// For chromosomes of different lengths, the strategy handles variable-length genes appropriately
/// by taking genes from the available parent when one parent is shorter.
/// </summary>
/// <typeparam name="T">The type of genes in the chromosome</typeparam>
public class UniformCrossoverStrategy<T> : BaseCrossoverStrategy<T>
{
    /// <summary>
    /// Performs uniform crossover on a couple of chromosomes to produce one offspring.
    /// Each gene is independently selected from either parent with 50% probability.
    /// </summary>
    /// <param name="couple">The pair of parent chromosomes to crossover</param>
    /// <param name="random">Random number generator for gene selection</param>
    /// <returns>One offspring chromosome resulting from the uniform crossover operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when the random parameter is null</exception>
    /// <exception cref="InvalidChromosomeException">Thrown when either parent chromosome has null genes</exception>
    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        if (couple.IndividualA?.Genes is null)
            throw new InvalidChromosomeException("Parent A has null genes collection.");
            
        if (couple.IndividualB?.Genes is null)
            throw new InvalidChromosomeException("Parent B has null genes collection.");
        
        // Determine the maximum length to ensure all genetic material is considered
        var maxLength = Math.Max(couple.IndividualA.Genes.Count, couple.IndividualB.Genes.Count);
        var minLength = Math.Min(couple.IndividualA.Genes.Count, couple.IndividualB.Genes.Count);
        
        // Start with the longer parent as the base
        var offspring = couple.IndividualA.Genes.Count >= couple.IndividualB.Genes.Count 
            ? couple.IndividualA.DeepCopy() 
            : couple.IndividualB.DeepCopy();
        
        // Perform uniform crossover for overlapping gene positions
        PerformUniformCrossoverInOverlapRegion(offspring, couple, random, minLength);
        
        // Handle non-overlapping region by copying from the longer parent
        CopyNonOverlappingGenes(offspring, couple, minLength, maxLength);
        
        yield return offspring;
    }
    
    /// <summary>
    /// Performs uniform crossover in the region where both parents have genes.
    /// Each gene position has a 50% chance of being selected from either parent.
    /// </summary>
    /// <param name="offspring">The offspring chromosome being constructed</param>
    /// <param name="couple">The parent chromosomes</param>
    /// <param name="random">Random number generator</param>
    /// <param name="minLength">The length of the shorter parent</param>
    private static void PerformUniformCrossoverInOverlapRegion(Chromosome<T> offspring, Couple<T> couple, Random random, int minLength)
    {
        for (int i = 0; i < minLength; i++)
        {
            // 50% chance to select from either parent
            // Using random.NextDouble() >= 0.5 for consistent behavior with existing tests
            offspring.Genes[i] = random.NextDouble() >= 0.5 
                ? couple.IndividualA.Genes[i] 
                : couple.IndividualB.Genes[i];
        }
    }
    
    /// <summary>
    /// Copies genes from the longer parent to fill the non-overlapping region.
    /// </summary>
    /// <param name="offspring">The offspring chromosome being constructed</param>
    /// <param name="couple">The parent chromosomes</param>
    /// <param name="minLength">The length of the shorter parent</param>
    /// <param name="maxLength">The length of the longer parent</param>
    private static void CopyNonOverlappingGenes(Chromosome<T> offspring, Couple<T> couple, int minLength, int maxLength)
    {
        if (minLength >= maxLength) return; // No non-overlapping region
        
        // Determine which parent is longer and copy its remaining genes
        var longerParent = couple.IndividualA.Genes.Count > couple.IndividualB.Genes.Count 
            ? couple.IndividualA 
            : couple.IndividualB;
            
        for (int i = minLength; i < maxLength; i++)
        {
            offspring.Genes[i] = longerParent.Genes[i];
        }
    }
}
