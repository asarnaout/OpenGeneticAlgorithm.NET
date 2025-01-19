using OpenGA.Net.Exceptions;

namespace OpenGA.Net.CrossoverStrategies;

public class OnePointCrossoverStrategy<T> : BaseCrossoverStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        if (couple.IndividualA.Genes.Length == 0 || couple.IndividualB.Genes.Length == 0)
        {
            throw new InvalidChromosomeException("Attempting One Point Crossover on an invalid chromosome. All chromosomes must have at least one gene for one point crossover.");
        }

        var crossoverPoint = GetCrossoverPoint(couple, random);

        Chromosome<T> offspringA = couple.IndividualA.DeepCopy(), offspringB = couple.IndividualB.DeepCopy();
        
        SwapGenes(crossoverPoint, offspringA, couple.IndividualB);
        SwapGenes(crossoverPoint, offspringB, couple.IndividualA);

        yield return offspringA;
        yield return offspringB;
    }

    /// <summary>
    /// Method decides the crossover point. Override to provide a custom implementation.
    /// </summary>
    protected internal virtual int GetCrossoverPoint(Couple<T> couple, Random random) 
        => random.Next(1, Math.Min(couple.IndividualA.Genes.Length, couple.IndividualB.Genes.Length));

    private static void SwapGenes(int crossoverPoint, Chromosome<T> offspring, Chromosome<T> parent)
    {
        var replacementGenes = crossoverPoint <= parent.Genes.Length? parent.Genes[crossoverPoint..] : [];

        if (replacementGenes.Length > 0 && crossoverPoint >= offspring.Genes.Length)
        {
            T[] newGenes = new T[offspring.Genes.Length + replacementGenes.Length];

            Array.Copy(offspring.Genes, newGenes, offspring.Genes.Length);

            offspring.Genes = newGenes;
        }

        foreach (var gene in replacementGenes)
        {
            offspring.Genes[crossoverPoint++] = gene;
        }

        offspring.Genes = offspring.Genes.Take(crossoverPoint).ToArray();
    }
}