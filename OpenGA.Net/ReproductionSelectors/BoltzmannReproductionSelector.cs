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
/// 4. Temperature starts at the specified initial value and decays over time using the specified decay rate
/// 5. Temperature never goes below 0 (for linear decay) or approaches 0 asymptotically (for exponential decay)
/// 
/// This approach provides a smooth transition from exploration to exploitation over time.
/// </summary>
public class BoltzmannReproductionSelector<T>(double temperatureDecayRate, double initialTemperature = 1.0, bool useExponentialDecay = true) : BaseReproductionSelector<T>
{
    private readonly double _temperatureDecayRate = temperatureDecayRate;
    private readonly double _initialTemperature = initialTemperature;
    private readonly bool _useExponentialDecay = useExponentialDecay;

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

        // Calculate current temperature with decay
        var currentTemperature = _useExponentialDecay 
            ? _initialTemperature * Math.Exp(-_temperatureDecayRate * currentEpoch)
            : Math.Max(0, _initialTemperature - (_temperatureDecayRate * currentEpoch));
        
        // If temperature reaches 0 (only possible with linear decay), use a very small positive value to avoid division by zero
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
