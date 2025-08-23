namespace OpenGA.Net.ReproductionSelectors;

/// <summary>
/// Boltzmann Selection reproduction selector that uses temperature-based selection probabilities with decay.
/// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter
/// that decays over epochs.
/// 
/// In Boltzmann selection:
/// 1. Selection probability is calculated using exp(fitness / temperature)
/// 2. Higher temperature leads to more uniform selection (exploration)
/// 3. Lower temperature leads to more elitist selection (exploitation)
/// 4. Temperature starts at 1.0 and decays over time using the specified decay rate
/// 5. Temperature never goes below 0
/// 
/// This approach provides a smooth transition from exploration to exploitation over time.
/// </summary>
public class BoltzmannReproductionSelector<T>(double temperatureDecayRate) : BaseReproductionSelector<T>
{
    private readonly double _temperatureDecayRate = temperatureDecayRate;
    private const double InitialTemperature = 1.0;

    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        // Default to epoch 0 when epoch information is not available
        return SelectMatingPairs(population, random, minimumNumberOfCouples, 0);
    }

    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples, int currentEpoch)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        // Calculate current temperature with decay, ensuring it never goes below 0
        var currentTemperature = Math.Max(0, InitialTemperature - (_temperatureDecayRate * currentEpoch));
        
        // If temperature reaches 0, use a very small positive value to avoid division by zero
        if (currentTemperature == 0)
        {
            currentTemperature = double.Epsilon;
        }

        // Calculate Boltzmann weights: exp(fitness / temperature)
        // To avoid numerical overflow/underflow, we normalize by subtracting the maximum fitness
        var maxFitness = population.Max(x => x.Fitness);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, 
                chromosome => Math.Exp((chromosome.Fitness - maxFitness) / currentTemperature)));
    }
}
