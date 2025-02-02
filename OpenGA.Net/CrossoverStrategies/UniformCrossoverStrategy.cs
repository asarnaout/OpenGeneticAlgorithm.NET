
namespace OpenGA.Net.CrossoverStrategies;

public class UniformCrossoverStrategy<T> : BaseCrossoverStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        var offspring = couple.IndividualA.Genes.Count >= couple.IndividualB.Genes.Count? 
                            couple.IndividualA.DeepCopy() : couple.IndividualB.DeepCopy();

        for (var i = 0; i < offspring.Genes.Count; i++)
        {
            if (i >= couple.IndividualA.Genes.Count)
            {
                offspring.Genes[i] = couple.IndividualB.Genes[i];
                continue;
            }

            if (i >= couple.IndividualB.Genes.Count)
            {
                offspring.Genes[i] = couple.IndividualA.Genes[i];
                continue;
            }

            var coinToss = random.NextDouble();

            offspring.Genes[i] = coinToss >= 0.5d? couple.IndividualA.Genes[i] : couple.IndividualB.Genes[i];            
        }

        yield return offspring;
    }
}
