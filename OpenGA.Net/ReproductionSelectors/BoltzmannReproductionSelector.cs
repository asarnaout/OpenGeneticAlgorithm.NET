namespace OpenGA.Net.ReproductionSelectors;

public class BoltzmannReproductionSelector<T> : BaseReproductionSelector<T>
{
    public override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, ReproductionSelectorConfiguration config, Random random, int minimumNumberOfCouples)
    {
        throw new NotImplementedException();
    }
}
