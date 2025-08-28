namespace OpenGA.Net.ParentSelectors;

public class BoltzmannParentSelector<T>(double temperatureDecayRate, double initialTemperature = 1.0, bool useExponentialDecay = true) : BaseParentSelector<T>
{
    private readonly double _temperatureDecayRate = temperatureDecayRate;
    private readonly double _initialTemperature = initialTemperature;
    private readonly bool _useExponentialDecay = useExponentialDecay;

    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
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

        var currentTemperature = _useExponentialDecay 
            ? _initialTemperature * Math.Exp(-_temperatureDecayRate * currentEpoch)
            : Math.Max(0, _initialTemperature - (_temperatureDecayRate * currentEpoch));
        
        if (currentTemperature == 0)
        {
            currentTemperature = double.Epsilon;
        }

        var maxFitness = population.Max(x => x.Fitness);
        
        return CreateStochasticCouples(population, random, minimumNumberOfCouples, 
            () => WeightedRouletteWheel<Chromosome<T>>.Init(population, 
                chromosome => Math.Exp((chromosome.Fitness - maxFitness) / currentTemperature)));
    }
}
