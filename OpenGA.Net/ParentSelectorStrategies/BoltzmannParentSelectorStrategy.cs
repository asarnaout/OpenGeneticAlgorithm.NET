namespace OpenGA.Net.ParentSelectorStrategies;

public class BoltzmannParentSelectorStrategy<T>(double temperatureDecayRate, double initialTemperature = 1.0, bool useExponentialDecay = true) : BaseParentSelectorStrategy<T>
{
    private readonly double _temperatureDecayRate = temperatureDecayRate;
    private readonly double _initialTemperature = initialTemperature;
    private readonly bool _useExponentialDecay = useExponentialDecay;

    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        return await SelectMatingPairsAsync(population, random, minimumNumberOfCouples, 0);
    }

    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples, int currentEpoch)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        var currentTemperature = _useExponentialDecay 
            ? _initialTemperature * Math.Exp(-_temperatureDecayRate * currentEpoch)
            : Math.Max(0, _initialTemperature - (_temperatureDecayRate * currentEpoch));
        
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
        
        var maxFitness = fitnessValues.Max();
        
        // Create a dictionary mapping chromosomes to their fitness values for O(1) lookup
        var fitnessLookup = new Dictionary<Chromosome<T>, double>();
        for (int i = 0; i < population.Length; i++)
        {
            fitnessLookup[population[i]] = fitnessValues[i];
        }
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, 
                chromosome => Math.Exp((fitnessLookup[chromosome] - maxFitness) / currentTemperature)));
    }
}
