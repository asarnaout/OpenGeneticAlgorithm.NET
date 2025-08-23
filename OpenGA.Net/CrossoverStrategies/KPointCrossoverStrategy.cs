
using OpenGA.Net.Exceptions;

namespace OpenGA.Net.CrossoverStrategies;

public class KPointCrossoverStrategy<T>(int numberOfPoints) : BaseCrossoverStrategy<T>
{
    internal int NumberOfPoints { get; set; } = numberOfPoints;

    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        if (couple.IndividualA.Genes.Count == 0 || couple.IndividualB.Genes.Count == 0)
        {
            throw new InvalidChromosomeException($"Attempting {NumberOfPoints}-Point Crossover on an invalid chromosome. All chromosomes must have at least one gene for one point crossover.");
        }

        if (NumberOfPoints > couple.IndividualA.Genes.Count - 1 && NumberOfPoints > couple.IndividualB.Genes.Count - 1)
        {
            throw new InvalidChromosomeException($"Attempting {NumberOfPoints}-Point Crossover on chromosomes that do not have at least {NumberOfPoints + 1} genes. Ensure that chromosomes would have at least {NumberOfPoints + 1} genes present for crossover.");
        }

        var crossoverPoints = new HashSet<int>();

        while (crossoverPoints.Count < NumberOfPoints)
        {
            crossoverPoints.Add(random.Next(1, Math.Min(couple.IndividualA.Genes.Count, couple.IndividualB.Genes.Count) + 1));
        }

        // Sort crossover points to ensure proper segment ordering
        var sortedCrossoverPoints = crossoverPoints.OrderBy(x => x).ToList();

        // Create offspring by alternating segments between parents
        var offspringA = CreateOffspring(couple.IndividualA, couple.IndividualB, sortedCrossoverPoints);
        var offspringB = CreateOffspring(couple.IndividualB, couple.IndividualA, sortedCrossoverPoints);

        yield return offspringA;
        yield return offspringB;
    }

    /// <summary>
    /// Creates a single offspring chromosome by combining genes from two parents using multiple crossover points.
    /// Genes are alternated between parents at each crossover point.
    /// </summary>
    /// <param name="primaryParent">Parent contributing genes in the first segment</param>
    /// <param name="secondaryParent">Parent contributing genes in alternating segments</param>
    /// <param name="sortedCrossoverPoints">List of crossover points in ascending order</param>
    /// <returns>A new offspring chromosome with combined genetic material</returns>
    private static Chromosome<T> CreateOffspring(Chromosome<T> primaryParent, Chromosome<T> secondaryParent, IList<int> sortedCrossoverPoints)
    {
        var offspring = primaryParent.DeepCopy();
        offspring.ResetAge();

        // Determine the length of the offspring chromosome
        var maxLength = Math.Max(primaryParent.Genes.Count, secondaryParent.Genes.Count);
        
        // Resize offspring gene list if necessary
        if (offspring.Genes.Count < maxLength)
        {
            var genesToAdd = maxLength - offspring.Genes.Count;
            for (int i = 0; i < genesToAdd; i++)
            {
                offspring.Genes.Add(default!);
            }
        }
        else if (offspring.Genes.Count > maxLength)
        {
            while (offspring.Genes.Count > maxLength)
            {
                offspring.Genes.RemoveAt(offspring.Genes.Count - 1);
            }
        }

        // Apply k-point crossover by alternating segments between parents
        bool useSecondaryParent = false;
        int segmentStart = 0;

        foreach (var crossoverPoint in sortedCrossoverPoints)
        {
            if (crossoverPoint > maxLength) break;

            CopyGeneSegment(offspring.Genes, 
                           useSecondaryParent ? secondaryParent.Genes : primaryParent.Genes,
                           segmentStart, 
                           Math.Min(crossoverPoint, maxLength));

            segmentStart = crossoverPoint;
            useSecondaryParent = !useSecondaryParent; // Alternate between parents
        }

        // Copy the final segment
        if (segmentStart < maxLength)
        {
            CopyGeneSegment(offspring.Genes,
                           useSecondaryParent ? secondaryParent.Genes : primaryParent.Genes,
                           segmentStart,
                           maxLength);
        }

        return offspring;
    }

    /// <summary>
    /// Copies a segment of genes from a source to a destination.
    /// </summary>
    /// <param name="destinationGenes">The destination gene list</param>
    /// <param name="sourceGenes">The source gene list</param>
    /// <param name="startIndex">The starting index (inclusive)</param>
    /// <param name="endIndex">The ending index (exclusive)</param>
    private static void CopyGeneSegment(IList<T> destinationGenes, IList<T> sourceGenes, int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex && i < sourceGenes.Count && i < destinationGenes.Count; i++)
        {
            destinationGenes[i] = sourceGenes[i];
        }
    }
}
