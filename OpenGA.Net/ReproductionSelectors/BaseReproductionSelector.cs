namespace OpenGA.Net.ReproductionSelectors;

public abstract class BaseReproductionSelector<T>
{
    protected internal abstract IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples);

    internal virtual IEnumerable<Couple<T>> CreateStochasticCouples(IList<Chromosome<T>> candidates, Random random, int minimumNumberOfCouples, Func<WeightedRouletteWheel<Chromosome<T>>> rouletteWheelBuilder)
    {
        if (candidates.Count <= 1)
        {
            yield break;
        }

        candidates = [.. candidates.OrderBy(x => random.Next())];

        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            var rouletteWheel = rouletteWheelBuilder();

            var winner1 = rouletteWheel.SpinAndReadjustWheel();
            var winner2 = rouletteWheel.SpinAndReadjustWheel();

            yield return Couple<T>.Pair(winner1, winner2);
        }
    }

    internal virtual IEnumerable<Couple<T>> GenerateCouplesFromATwoIndividualPopulation(IList<Chromosome<T>> candidates, int minimumNumberOfCouples)
    {
        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            yield return Couple<T>.Pair(candidates[0], candidates[1]);
        }
    }
}
