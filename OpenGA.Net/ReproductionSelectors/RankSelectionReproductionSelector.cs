namespace OpenGA.Net.ReproductionSelectors;

/// <summary>
/// Rank Selection reproduction selector that assigns selection probability based on fitness rank rather than raw fitness values.
/// This strategy reduces the selection pressure compared to fitness-weighted selection by normalizing the fitness differences.
/// 
/// In rank selection:
/// 1. Chromosomes are sorted by fitness (worst to best)
/// 2. Ranks are assigned (1 = worst, N = best where N is population size)
/// 3. Selection probability is proportional to rank, not raw fitness
/// 
/// This approach prevents highly fit individuals from completely dominating selection while still favoring better chromosomes.
/// </summary>
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

        // Assign ranks based on fitness: worst chromosome gets rank 1, best gets rank N
        // This creates a linear selection pressure where rank (not fitness) determines selection probability
        var rankedPopulation = population.OrderBy(x => x.CalculateFitness())
                                          .Select((chromosome, index) => new { Chromosome = chromosome, Rank = index + 1 })
                                          .ToDictionary(x => x.Chromosome, y => y.Rank);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, chromosome => rankedPopulation[chromosome]));
    }
}
