namespace OpenGA.Net.ParentSelectorStrategies;

public class FitnessWeightedRouletteWheelParentSelectorStrategy<T> : BaseParentSelectorStrategy<T>
{
    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        // Get fitness values for all chromosomes
        var fitnessValues = new double[population.Length];
        for (int i = 0; i < population.Length; i++)
        {
            fitnessValues[i] = await population[i].GetCachedFitnessAsync();
        }

        return CreateStochasticCouples(population, random, minimumNumberOfCouples, () => 
            WeightedRouletteWheel<Chromosome<T>>.Init(population, chromosome => 
            {
                var index = Array.IndexOf(population, chromosome);
                return fitnessValues[index];
            }));
    }
}
