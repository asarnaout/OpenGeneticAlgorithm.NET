using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.CrossoverStrategies;

public class CrossoverStrategyConfiguration<T>
{
    internal BaseCrossoverStrategy<T>? CrossoverStrategy { get; private set; }

    private readonly OperatorSelectionPolicyConfiguration _policyConfig = new();

    /// <summary>
    /// A point is chosen at random, and all the genes following that point are swapped between both parent chromosomes to produce two new child chromosomes
    /// </summary>
    public void OnePointCrossover()
    {
        var result = new OnePointCrossoverStrategy<T>();
        CrossoverStrategy = result;
    }

    /// <summary>
    /// A child chromosome is created by copying gene by gene from either parents (on a random basis).
    /// </summary>
    public void UniformCrossover()
    {
        var result = new UniformCrossoverStrategy<T>();
        CrossoverStrategy = result;
    }

    /// <summary>
    /// Multiple points are chosen at random, and genes are alternated between parents at each crossover point to produce two new child chromosomes.
    /// </summary>
    /// <param name="numberOfPoints">The number of crossover points to use. Must be greater than 0.</param>
    public void KPointCrossover(int numberOfPoints)
    {
        if (numberOfPoints <= 0)
        {
            throw new ArgumentException("Number of crossover points must be greater than 0.", nameof(numberOfPoints));
        }

        var result = new KPointCrossoverStrategy<T>(numberOfPoints);
        CrossoverStrategy = result;
    }

    /// <summary>
    /// Apply a custom strategy for crossing over chromosomes. Requires an instance of a subclass of <see cref="BaseCrossoverStrategy<T>">BaseCrossoverStrategy<T></see>
    /// to dictate which how a Couple of Chromosomes can reproduce a new set of Chromosomes.
    /// </summary>
    public void CustomCrossover(BaseCrossoverStrategy<T> crossoverStrategy)
    {
        ArgumentNullException.ThrowIfNull(crossoverStrategy, nameof(crossoverStrategy));
        CrossoverStrategy = crossoverStrategy;
    }

    internal void ValidateAndDefault(Random random)
    {
        if (CrossoverStrategy is null)
        {
            OnePointCrossover();
        }
        
        _policyConfig.FirstChoice();

        _policyConfig.Policy!.ApplyOperators([CrossoverStrategy!], random);
    }

    internal OperatorSelectionPolicy GetCrossoverSelectionPolicy()
    {
        return _policyConfig.Policy;
    }
}
