namespace OpenGA.Net.ParentSelectorStrategies;

public class RankSelectionParentSelectorStrategy<T> : BaseParentSelectorStrategy<T>
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
        var populationWithFitness = new List<(Chromosome<T> chromosome, double fitness)>();
        foreach (var chromosome in population)
        {
            var fitness = await chromosome.GetCachedFitnessAsync();
            populationWithFitness.Add((chromosome, fitness));
        }

        var rankedPopulation = populationWithFitness.OrderBy(x => x.fitness)
                                          .Select((item, index) => new { item.chromosome, Rank = index + 1 })
                                          .ToDictionary(x => x.chromosome, y => y.Rank);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, chromosome => rankedPopulation[chromosome]));
    }
}
