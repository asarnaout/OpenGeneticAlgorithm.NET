namespace OpenGA.Net.ReproductionSelectors;

public class RankSelectionReproductionSelector<T> : BaseReproductionSelector<T>
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

        var sortedPopulation = population.OrderBy(x => x.CalculateFitness())
                                          .Select((x, i) => new { Value = x, Index = i + 1 })
                                          .ToDictionary(x => x.Value, y => y.Index);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, d => sortedPopulation[d]));
    }
}
