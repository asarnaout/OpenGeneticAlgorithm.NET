namespace OpenGA.Net.ReproductionSelectors;

public class BoltzmannReproductionSelector<T> : BaseReproductionSelector<T>
{
    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        throw new NotImplementedException();
    }
}
