namespace OpenGA.Net.ParentSelectors;

public class RankSelectionParentSelector<T> : BaseParentSelector<T>
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

        var rankedPopulation = population.OrderBy(x => x.Fitness)
                                          .Select((chromosome, index) => new { Chromosome = chromosome, Rank = index + 1 })
                                          .ToDictionary(x => x.Chromosome, y => y.Rank);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, chromosome => rankedPopulation[chromosome]));
    }
}
