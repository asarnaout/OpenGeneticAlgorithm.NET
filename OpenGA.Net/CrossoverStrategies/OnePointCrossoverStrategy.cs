using OpenGA.Net.Exceptions;

namespace OpenGA.Net.CrossoverStrategies;

/// <summary>
/// Implements one-point crossover genetic operator for chromosomes.
/// 
/// One-point crossover works by selecting a random crossover point and creating two offspring:
/// - Offspring A: Takes genes [0...crossoverPoint-1] from Parent A and [crossoverPoint...end] from Parent B
/// - Offspring B: Takes genes [0...crossoverPoint-1] from Parent B and [crossoverPoint...end] from Parent A
/// 
/// This strategy can handle variable-length chromosomes by adapting the crossover point to the shorter parent
/// and extending or truncating the offspring as needed.
/// </summary>
/// <typeparam name="T">The type of genes in the chromosome</typeparam>
public class OnePointCrossoverStrategy<T> : BaseCrossoverStrategy<T>
{
    /// <summary>
    /// Performs one-point crossover on a couple of chromosomes to produce two offspring.
    /// </summary>
    /// <param name="couple">The pair of parent chromosomes to crossover</param>
    /// <param name="random">Random number generator for selecting the crossover point</param>
    /// <returns>Two offspring chromosomes resulting from the crossover operation</returns>
    /// <exception cref="InvalidChromosomeException">
    /// Thrown when either parent chromosome has fewer than 2 genes, making crossover impossible
    /// </exception>
    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        // Validate that both parents have enough genes for meaningful crossover
        if (couple.IndividualA.Genes.Count <= 1 || couple.IndividualB.Genes.Count <= 1)
        {
            throw new InvalidChromosomeException(
                "Attempting One Point Crossover on an invalid chromosome. " +
                "All chromosomes must have at least 2 genes for one point crossover.");
        }

        var crossoverPoint = GetCrossoverPoint(couple, random);

        // Create offspring using efficient direct gene copying
        var offspringA = CreateOffspring(couple.IndividualA, couple.IndividualB, crossoverPoint);
        var offspringB = CreateOffspring(couple.IndividualB, couple.IndividualA, crossoverPoint);

        yield return offspringA;
        yield return offspringB;
    }

    /// <summary>
    /// Determines the crossover point for the genetic crossover operation.
    /// The crossover point is selected randomly between 1 and the length of the shorter parent,
    /// ensuring that both parents contribute at least one gene to each offspring.
    /// </summary>
    /// <param name="couple">The pair of parent chromosomes</param>
    /// <param name="random">Random number generator</param>
    /// <returns>The index where crossover should occur (1-based, meaning genes [0...crossoverPoint-1] come from first parent)</returns>
    protected internal virtual int GetCrossoverPoint(Couple<T> couple, Random random)
    {
        var minLength = Math.Min(couple.IndividualA.Genes.Count, couple.IndividualB.Genes.Count);
        
        // Crossover point should be between 1 and minLength (inclusive)
        // This ensures both parents contribute at least one gene
        return random.Next(1, minLength + 1);
    }

    /// <summary>
    /// Creates a single offspring chromosome by combining genes from two parents at the specified crossover point.
    /// Genes [0...crossoverPoint-1] come from primaryParent, genes [crossoverPoint...end] come from secondaryParent.
    /// </summary>
    /// <param name="primaryParent">Parent contributing genes before the crossover point</param>
    /// <param name="secondaryParent">Parent contributing genes after the crossover point</param>
    /// <param name="crossoverPoint">The point where genetic material switches from primary to secondary parent</param>
    /// <returns>A new offspring chromosome with combined genetic material</returns>
    private static Chromosome<T> CreateOffspring(Chromosome<T> primaryParent, Chromosome<T> secondaryParent, int crossoverPoint)
    {
        var offspring = primaryParent.DeepCopy();
        
        // Calculate the target length for the offspring
        // Take genes up to crossoverPoint from primary, remainder from secondary
        var primaryGeneCount = Math.Min(crossoverPoint, primaryParent.Genes.Count);
        var secondaryGeneCount = Math.Max(0, secondaryParent.Genes.Count - crossoverPoint);
        var targetLength = primaryGeneCount + secondaryGeneCount;
        
        // Resize offspring gene list if necessary
        if (offspring.Genes.Count > targetLength)
        {
            RemoveGenesFromEnd(offspring.Genes, offspring.Genes.Count - targetLength);
        }
        else if (offspring.Genes.Count < targetLength)
        {
            // Add placeholder genes that will be overwritten
            var genesToAdd = targetLength - offspring.Genes.Count;
            for (int i = 0; i < genesToAdd; i++)
            {
                offspring.Genes.Add(default!);
            }
        }
        
        // Copy genes from secondary parent starting at crossover point
        CopyGenesFromSecondaryParent(offspring.Genes, secondaryParent.Genes, crossoverPoint);
        
        return offspring;
    }

    /// <summary>
    /// Efficiently removes genes from the end of a gene list.
    /// </summary>
    /// <param name="genes">The gene list to modify</param>
    /// <param name="countToRemove">Number of genes to remove from the end</param>
    private static void RemoveGenesFromEnd(IList<T> genes, int countToRemove)
    {
        // Remove from the end to avoid shifting elements
        for (int i = 0; i < countToRemove; i++)
        {
            genes.RemoveAt(genes.Count - 1);
        }
    }

    /// <summary>
    /// Copies genes from the secondary parent to the offspring starting at the crossover point.
    /// </summary>
    /// <param name="offspringGenes">The offspring's gene list to modify</param>
    /// <param name="secondaryParentGenes">The secondary parent's genes to copy from</param>
    /// <param name="crossoverPoint">The starting point for copying genes</param>
    private static void CopyGenesFromSecondaryParent(IList<T> offspringGenes, IList<T> secondaryParentGenes, int crossoverPoint)
    {
        var offspringIndex = crossoverPoint;
        var secondaryIndex = crossoverPoint;
        
        // Copy remaining genes from secondary parent
        while (secondaryIndex < secondaryParentGenes.Count && offspringIndex < offspringGenes.Count)
        {
            offspringGenes[offspringIndex] = secondaryParentGenes[secondaryIndex];
            offspringIndex++;
            secondaryIndex++;
        }
    }
}