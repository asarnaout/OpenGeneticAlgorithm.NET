namespace OpenGA.Net.CrossoverStrategies;

public class OnePointCrossoverStrategy<T> : BaseCrossoverStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple)
    {
        throw new NotImplementedException();
    }
}
