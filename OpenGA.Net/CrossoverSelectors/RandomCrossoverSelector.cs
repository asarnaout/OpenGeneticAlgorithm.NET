namespace OpenGA.Net.CrossoverSelectors;

public class RandomCrossoverSelector<T> : BaseCrossoverSelector<T>
{
    public override IEnumerable<Couple<T>> SelectParents(Chromosome<T>[] population, CrossoverConfiguration config, Random random, int minimumNumberOfCouples)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        return CreateStochasticCouples(population, random, minimumNumberOfCouples, () => WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(population));
    }
}
