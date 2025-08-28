namespace OpenGA.Net.CrossoverStrategies;

/// <summary>
/// Configuration class specifically for multiple crossover strategies with weight support.
/// This class provides the same crossover strategy methods as CrossoverStrategyConfiguration
/// but with optional weight parameters for use in multi-strategy scenarios.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class MultiCrossoverStrategyConfiguration<T>
{
    internal IList<BaseCrossoverStrategy<T>> CrossoverStrategies = [];

    /// <summary>
    /// A point is chosen at random, and all the genes following that point are swapped between both parent chromosomes to produce two new child chromosomes
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> OnePointCrossover(float? customWeight = null)
    {
        var result = new OnePointCrossoverStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        CrossoverStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// A child chromosome is created by copying gene by gene from either parents (on a random basis).
    /// </summary>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> UniformCrossover(float? customWeight = null)
    {
        var result = new UniformCrossoverStrategy<T>();
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        CrossoverStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Multiple points are chosen at random, and genes are alternated between parents at each crossover point to produce two new child chromosomes.
    /// </summary>
    /// <param name="numberOfPoints">The number of crossover points to use. Must be greater than 0.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> KPointCrossover(int numberOfPoints, float? customWeight = null)
    {
        if (numberOfPoints <= 0)
        {
            throw new ArgumentException("Number of crossover points must be greater than 0.", nameof(numberOfPoints));
        }

        var result = new KPointCrossoverStrategy<T>(numberOfPoints);
        if (customWeight.HasValue)
        {
            result.WithCustomWeight(customWeight.Value);
        }
        CrossoverStrategies.Add(result);
        return this;
    }

    /// <summary>
    /// Apply a custom strategy for crossing over chromosomes. Requires an instance of a subclass of <see cref="BaseCrossoverStrategy<T>">BaseCrossoverStrategy<T></see>
    /// to dictate which how a Couple of Chromosomes can reproduce a new set of Chromosomes.
    /// </summary>
    /// <param name="crossoverStrategy">The custom crossover strategy instance to add.</param>
    /// <param name="customWeight">Optional custom weight for this strategy when used with multiple strategies. Higher weights increase selection probability.</param>
    public MultiCrossoverStrategyConfiguration<T> CustomCrossover(BaseCrossoverStrategy<T> crossoverStrategy, float? customWeight = null)
    {
        ArgumentNullException.ThrowIfNull(crossoverStrategy, nameof(crossoverStrategy));
        
        if (customWeight.HasValue)
        {
            crossoverStrategy.WithCustomWeight(customWeight.Value);
        }
        
        CrossoverStrategies.Add(crossoverStrategy);
        return this;
    }
}
