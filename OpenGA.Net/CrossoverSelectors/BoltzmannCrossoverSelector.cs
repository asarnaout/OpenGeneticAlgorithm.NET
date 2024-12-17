namespace OpenGA.Net.CrossoverSelectors;

public class BoltzmannCrossoverSelector<T> : BaseCrossoverSelector<T>
{
    public override IEnumerable<Couple<T>> SelectParents(Chromosome<T>[] population, CrossoverConfiguration config, Random random, int minimumNumberOfCouples)
    {
        throw new NotImplementedException();
    }
}
