
namespace OpenGA.Net.CrossoverStrategies;

public class KPointCrossoverStrategy<T>(int numberOfPoints) : BaseCrossoverStrategy<T>
{
    internal int NumberOfPoints { get; set; } = numberOfPoints;

    protected internal override IEnumerable<Chromosome<T>> Crossover(Couple<T> couple, Random random)
    {
        throw new NotImplementedException();
    }
}
