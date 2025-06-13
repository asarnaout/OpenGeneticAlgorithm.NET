namespace OpenGA.Net.CrossoverStrategies;

public class CrossoverStrategyConfiguration<T>
{
    internal BaseCrossoverStrategy<T> CrossoverStrategy = default!;

    /// <summary>
    /// A point is chosen at random, and all the genes following that point are swapped between both parent chromosomes to produce two new child chromosomes
    /// </summary>
    public BaseCrossoverStrategy<T> OnePointCrossover()
    {
        var result = new OnePointCrossoverStrategy<T>();
        CrossoverStrategy = result;
        return result;
    }

    /// <summary>
    /// A child chromosome is created by copying gene by gene from either parents (on a random basis).
    /// </summary>
    public BaseCrossoverStrategy<T> UniformCrossover()
    {
        var result = new UniformCrossoverStrategy<T>();
        CrossoverStrategy = result;
        return result;
    }

    /// <summary>
    /// Apply a custom strategy for crossing over chromosomes. Requires an instance of a subclass of <see cref="BaseCrossoverStrategy<T>">BaseCrossoverStrategy<T></see>
    /// to dictate which how a Couple of Chromosomes can reproduce a new set of Chromosomes.
    /// </summary>
    public BaseCrossoverStrategy<T> CustomCrossover(BaseCrossoverStrategy<T> crossoverStrategy)
    {
        ArgumentNullException.ThrowIfNull(crossoverStrategy, nameof(crossoverStrategy));
        CrossoverStrategy = crossoverStrategy;
        return crossoverStrategy;
    }
}
