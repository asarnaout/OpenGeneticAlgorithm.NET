using OpenGA.Net.Exceptions;

namespace OpenGA.Net.CrossoverStrategies;

public class OnePointCrossoverStrategy<T> : BaseCrossoverStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        if (couple.IndividualA.Genes.Count <= 1 || couple.IndividualB.Genes.Count <= 1)
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
        => random.Next(1, Math.Min(couple.IndividualA.Genes.Count, couple.IndividualB.Genes.Count) + 1);

    private static void SwapGenes(int crossoverPoint, Chromosome<T> offspring, Chromosome<T> parent)
    {
        foreach (var gene in parent.Genes.Skip(crossoverPoint))
        {
            if (crossoverPoint < offspring.Genes.Count)
            {
                offspring.Genes[crossoverPoint] = gene;
            }
            else
            {
                offspring.Genes.Add(gene);
            }

            ++crossoverPoint;
        }

        offspring.Genes = offspring.Genes.Take(crossoverPoint).ToList();
    }
}