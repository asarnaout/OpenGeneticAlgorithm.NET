namespace OpenGA.Net.ParentSelectorStrategies;

public abstract class BaseParentSelectorStrategy<T> : BaseOperator
{
    protected internal virtual async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples, int currentEpoch = 0)
    {
        return await SelectMatingPairsAsync(population, random, minimumNumberOfCouples);
    }

    internal virtual IEnumerable<Couple<T>> CreateStochasticCouples(IList<Chromosome<T>> candidates, Random random, int minimumNumberOfCouples, Func<WeightedRouletteWheel<Chromosome<T>>> rouletteWheelBuilder)
    {
        if (candidates.Count <= 1)
        {
            yield break;
        }

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
