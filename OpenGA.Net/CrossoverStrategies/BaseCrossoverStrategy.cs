namespace OpenGA.Net.CrossoverStrategies;

public abstract class BaseCrossoverStrategy<T> : BaseOperator
{
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
