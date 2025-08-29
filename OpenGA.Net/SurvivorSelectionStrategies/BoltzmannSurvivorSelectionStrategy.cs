using OpenGA.Net.Extensions;

namespace OpenGA.Net.SurvivorSelectionStrategies;

/// <summary>
/// Boltzmann survivor selection strategy that uses temperature-based elimination probabilities with decay.
/// This strategy applies the Boltzmann distribution to control elimination pressure through a temperature parameter
/// that decays over epochs.
/// 
/// In Boltzmann survivor selection:
/// 1. Elimination probability is calculated using exp((maxFitness - fitness) / temperature) (inverse fitness)
/// 2. Higher temperature leads to more uniform elimination (exploration)
/// 3. Lower temperature leads to more fitness-based elimination (exploitation)
/// 4. Temperature starts at the specified initial value and decays over time using the specified decay rate
/// 5. Temperature never goes below 0 (for linear decay) or approaches 0 asymptotically (for exponential decay)
/// 
/// This approach provides a smooth transition from exploration to exploitation over time for survivor selection.
/// </summary>
public class BoltzmannSurvivorSelectionStrategy<T>(double temperatureDecayRate, double initialTemperature = 1.0, bool useExponentialDecay = true) : BaseSurvivorSelectionStrategy<T>
{
    /// <summary>
    /// The recommended offspring generation rate for Boltzmann survivor selection strategy.
    /// This moderate turnover rate (40%) works well with temperature-controlled selection pressure.
    /// </summary>
    internal override float RecommendedOffspringGenerationRate => 0.4f;

    private readonly double _temperatureDecayRate = temperatureDecayRate;
    private readonly double _initialTemperature = initialTemperature;
    private readonly bool _useExponentialDecay = useExponentialDecay;

    /// <summary>
    /// Selects chromosomes for elimination using Boltzmann distribution with temperature decay.
    /// Uses inverse fitness weighting where chromosomes with lower fitness have higher probability of elimination.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="currentEpoch">The current epoch/generation number for temperature calculation</param>
    /// <returns>The chromosomes selected for elimination through Boltzmann selection</returns>
    protected internal override async Task<IEnumerable<Chromosome<T>>> SelectChromosomesForEliminationAsync(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random, 
        int currentEpoch = 0)
    {
        if (population.Length == 0 || offspring.Length == 0)
        {
            return Enumerable.Empty<Chromosome<T>>();
        }

        // We need to eliminate as many chromosomes as we have offspring
        var eliminationsNeeded = Math.Min(offspring.Length, population.Length);

        // Optimization: if we need to eliminate the entire population, just return it directly
        if (eliminationsNeeded == population.Length)
        {
            return population;
        }

        // Calculate temperature based on cooling schedule
        var currentTemperature = _useExponentialDecay 
            ? _initialTemperature * Math.Exp(-_temperatureDecayRate * currentEpoch)
            : Math.Max(0, _initialTemperature - (_temperatureDecayRate * currentEpoch));

        // If temperature reaches 0 (only possible with linear decay), use a very small positive value to avoid division by zero
        if (currentTemperature == 0)
        {
            currentTemperature = double.Epsilon;
        }

        // Get fitness values for all chromosomes
        var fitnessValues = new double[population.Length];
        for (int i = 0; i < population.Length; i++)
        {
            fitnessValues[i] = await population[i].GetCachedFitnessAsync();
        }

        // Calculate Boltzmann weights for elimination: exp((maxFitness - fitness) / temperature)
        // This gives higher elimination probability to lower fitness chromosomes
        var maxFitness = fitnessValues.Max();
        var minFitness = fitnessValues.Min();
        
        // To avoid numerical overflow/underflow, we normalize by using the fitness range
        var fitnessRange = maxFitness - minFitness;
        
        // If all chromosomes have the same fitness, eliminate randomly
        if (fitnessRange == 0)
        {
            var shuffledPopulation = population.FisherYatesShuffle(random);
            return shuffledPopulation.Take(eliminationsNeeded);
        }

        var candidatesForElimination = new List<Chromosome<T>>(eliminationsNeeded);
        
        // Create a dictionary mapping chromosomes to their fitness values for O(1) lookup
        var fitnessLookup = new Dictionary<Chromosome<T>, double>();
        for (int i = 0; i < population.Length; i++)
        {
            fitnessLookup[population[i]] = fitnessValues[i];
        }
        
        // Create weighted roulette wheel with inverse fitness for elimination
        var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(population.ToList(), 
            chromosome => Math.Exp((maxFitness - fitnessLookup[chromosome]) / currentTemperature));

        for (int i = 0; i < eliminationsNeeded; i++)
        {
            var selected = rouletteWheel.SpinAndReadjustWheel();
            candidatesForElimination.Add(selected);
        }

        return candidatesForElimination;
    }
}
