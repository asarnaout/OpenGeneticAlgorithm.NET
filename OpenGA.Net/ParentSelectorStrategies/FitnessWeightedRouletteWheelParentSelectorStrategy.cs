namespace OpenGA.Net.ParentSelectorStrategies;

public class FitnessWeightedRouletteWheelParentSelectorStrategy<T> : BaseParentSelectorStrategy<T>
{
    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        return CreateStochasticCouples(population, random, minimumNumberOfCouples, () => WeightedRouletteWheel<Chromosome<T>>.Init(population, d => d.Fitness));
    }
}
