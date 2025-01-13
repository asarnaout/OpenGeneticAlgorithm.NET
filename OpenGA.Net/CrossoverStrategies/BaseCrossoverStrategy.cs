namespace OpenGA.Net.CrossoverStrategies;

public abstract class BaseCrossoverStrategy<T>
{
    protected internal abstract IEnumerable<Chromosome<T>> Crossover(Couple<T> couple);
}
