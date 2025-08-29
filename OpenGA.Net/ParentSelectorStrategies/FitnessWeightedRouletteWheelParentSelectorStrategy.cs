namespace OpenGA.Net.ParentSelectorStrategies;

public class FitnessWeightedRouletteWheelParentSelectorStrategy<T> : BaseParentSelectorStrategy<T>
{
    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples, int currentEpoch = 0)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        // Create a dictionary mapping chromosomes to their fitness values for O(1) lookup
        var fitnessLookup = new Dictionary<Chromosome<T>, double>();
        for (int i = 0; i < population.Length; i++)
        {
            fitnessLookup[population[i]] = await population[i].GetCachedFitnessAsync();
        }

        return CreateStochasticCouples(population, random, minimumNumberOfCouples, () => 
            WeightedRouletteWheel<Chromosome<T>>.Init(population, chromosome => fitnessLookup[chromosome]));
    }
}
