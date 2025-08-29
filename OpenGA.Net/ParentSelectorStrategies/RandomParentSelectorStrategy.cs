namespace OpenGA.Net.ParentSelectorStrategies;

public class RandomParentSelectorStrategy<T> : BaseParentSelectorStrategy<T>
{
    protected internal override Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples, int currentEpoch = 0)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumNumberOfCouples);

        if (population.Length <= 1)
        {
            return Task.FromResult(Enumerable.Empty<Couple<T>>());
        }

        if (population.Length == 2)
        {
            return Task.FromResult(GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples));
        }

        return Task.FromResult(CreateStochasticCouples(population, random, minimumNumberOfCouples,
            () => WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(population)));
    }
}
