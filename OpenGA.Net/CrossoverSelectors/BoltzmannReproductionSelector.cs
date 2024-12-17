namespace OpenGA.Net.CrossoverSelectors;

public class BoltzmannReproductionSelector<T> : BaseReproductionSelector<T>
{
    public override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, CrossoverConfiguration config, Random random, int minimumNumberOfCouples)
    {
        throw new NotImplementedException();
    }
}
