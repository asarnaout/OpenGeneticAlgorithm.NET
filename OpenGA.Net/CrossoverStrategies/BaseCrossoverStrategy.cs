namespace OpenGA.Net.CrossoverStrategies;

/// <summary>
/// Abstract base class for all crossover strategies in the genetic algorithm.
/// Crossover strategies define how parent chromosomes are combined to produce offspring
/// during the reproduction phase of the genetic algorithm.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public abstract class BaseCrossoverStrategy<T> : BaseOperator
{
    /// <summary>
    /// Abstract method that must be implemented by concrete crossover strategies to define
    /// how two parent chromosomes are combined to produce offspring. This method performs
    /// the core crossover operation based on the specific strategy's algorithm.
    /// </summary>
    /// <param name="couple">A pair of parent chromosomes that will undergo crossover to produce offspring.
    /// The couple contains two chromosomes (IndividualA and IndividualB) that have been selected
    /// by a parent selector strategy for reproduction.</param>
    /// <param name="random">Random number generator for stochastic crossover operations.
    /// Used for making probabilistic decisions during crossover, such as selecting crossover points
    /// or determining which genes to exchange between parents.</param>
    /// <returns>
    /// A collection of offspring chromosomes produced by crossing over the parent couple.
    /// The number of offspring returned depends on the specific crossover strategy:
    /// - Some strategies produce exactly one offspring
    /// - Others may produce two offspring (one from each possible combination)
    /// - Advanced strategies might produce variable numbers of offspring
    /// 
    /// Each offspring chromosome should:
    /// - Have its age reset to 0 (typically done via ResetAge())
    /// - Have its fitness invalidated (handled automatically by gene modifications)
    /// - Contain a valid combination of genes from both parents
    /// </returns>
    /// <remarks>
    /// Implementation guidelines for concrete crossover strategies:
    /// 
    /// 1. **Gene Exchange**: Define how genes are exchanged between parents (e.g., single-point, multi-point, uniform)
    /// 2. **Offspring Creation**: Use DeepCopyAsync() to create base offspring from one or both parents
    /// 3. **Age Reset**: Always call ResetAge() on newly created offspring
    /// 4. **Constraint Handling**: Ensure offspring satisfy problem-specific constraints
    /// 5. **Genetic Repair**: Consider calling GeneticRepairAsync() if crossover might create invalid gene combinations
    /// 
    /// Common crossover patterns:
    /// - **One-Point**: Split at random position, exchange tails
    /// - **Multi-Point**: Multiple split points for more genetic mixing
    /// - **Uniform**: Randomly choose each gene from either parent
    /// - **Order-based**: Preserve relative order for permutation problems
    /// </remarks>
    protected internal abstract Task<IEnumerable<Chromosome<T>>> CrossoverAsync(Couple<T> couple, Random random);

    /// <summary>
    /// Optional per-strategy override of the global crossover rate.
    /// When set, this value takes precedence over the runner-level crossover rate
    /// configured via CrossoverStrategyRegistration. A null value means no override
    /// and the global rate will be used.
    /// </summary>
    protected internal virtual float? CrossoverRateOverride { get; protected set; } = default;

    /// <summary>
    /// Overrides the global crossover rate for this specific crossover strategy instance.
    /// 
    /// Use this when a particular strategy should trigger crossover more or less often
    /// than the globally configured rate. Pass null to clear the override and revert to
    /// the global rate.
    /// </summary>
    /// <param name="value">A value between 0 and 1 (inclusive), or null to remove the override.</param>
    public void OverrideCrossoverRate(float? value)
    {
        CrossoverRateOverride = value;
    }
}
