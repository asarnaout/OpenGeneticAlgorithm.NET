namespace OpenGA.Net.CrossoverStrategies;

public class CrossoverStrategyConfiguration<T>
{
    internal BaseCrossoverStrategy<T> CrossoverStrategy = default!;

    /// <summary>
    /// A point is chosen at random, and all the genes following that point are swapped between both parent chromosomes to produce two new child chromosomes
    /// </summary>
    public BaseCrossoverStrategy<T> ApplyOnePointCrossoverStrategy()
    {
        var result = new OnePointCrossoverStrategy<T>();
        CrossoverStrategy = result;
        return result;
    }

    /// <summary>
    /// A child chromosome is created by copying gene by gene from either parents (on a random basis).
    /// </summary>
    public BaseCrossoverStrategy<T> ApplyUniformCrossoverStrategy()
    {
        var result = new UniformCrossoverStrategy<T>();
        CrossoverStrategy = result;
        return result;
    }
}
