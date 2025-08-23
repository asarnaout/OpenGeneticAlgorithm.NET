namespace OpenGA.Net.ReproductionSelectors;

/// <summary>
/// Boltzmann Selection reproduction selector that uses temperature-based selection probabilities.
/// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter.
/// 
/// In Boltzmann selection:
/// 1. Selection probability is calculated using exp(fitness / temperature)
/// 2. Higher temperature leads to more uniform selection (exploration)
/// 3. Lower temperature leads to more elitist selection (exploitation)
/// 4. Temperature can be adjusted over time for adaptive selection pressure
/// 
/// This approach provides a smooth transition between random and elitist selection strategies.
/// </summary>
public class BoltzmannReproductionSelector<T>(double temperature) : BaseReproductionSelector<T>
{
    private readonly double _temperature = temperature;

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

        // Calculate Boltzmann weights: exp(fitness / temperature)
        // To avoid numerical overflow/underflow, we normalize by subtracting the maximum fitness
        var maxFitness = population.Max(x => x.Fitness);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, 
                chromosome => Math.Exp((chromosome.Fitness - maxFitness) / _temperature)));
    }
}
