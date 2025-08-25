namespace OpenGA.Net.CrossoverStrategies;

public abstract class BaseCrossoverStrategy<T> : BaseOperator
{
    protected internal abstract IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random);
}
