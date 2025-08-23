
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

        // Create two offspring by alternating gene segments between parents
        var offspringA = couple.IndividualA.DeepCopy();
        var offspringB = couple.IndividualB.DeepCopy();
        
        // Reset age for both offspring chromosomes
        offspringA.ResetAge();
        offspringB.ResetAge();

        // TODO: Complete the k-point crossover implementation
        // For now, return the copied parents with reset ages
        yield return offspringA;
        yield return offspringB;
    }
}
