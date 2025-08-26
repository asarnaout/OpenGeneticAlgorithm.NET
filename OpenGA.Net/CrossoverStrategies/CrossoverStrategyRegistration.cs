using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.CrossoverStrategies;

public class CrossoverStrategyRegistration<T>
{
    private readonly CrossoverStrategyConfiguration<T> _crossoverStrategyConfig = new();

    private readonly OperatorSelectionPolicyConfiguration _crossoverSelectionPolicyConfig = new();

    private float _crossoverRate = 0.9f;

    public void RegisterSingleOperator(Action<CrossoverStrategyConfiguration<T>> singleRegistration)
    {
        ArgumentNullException.ThrowIfNull(singleRegistration, nameof(singleRegistration));

        singleRegistration(_crossoverStrategyConfig);
    }

    public CrossoverStrategyRegistration<T> RegisterMultiOperators(Action<CrossoverStrategyConfiguration<T>> configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        configurator(_crossoverStrategyConfig);

        return this;
    }

    public void WithPolicy(Action<OperatorSelectionPolicyConfiguration> policyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(policyConfigurator, nameof(policyConfigurator));

        policyConfigurator(_crossoverSelectionPolicyConfig);
    }

    /// <summary>
    /// The crossover rate dictates the likelihood of 2 mating parents producing an offspring. Defaults to 0.9 (90%).
    /// </summary>
    /// <param name="crossoverRate">Value should be between 0 and 1, where 0 indicates no chance of success in reproduction while 1 indicates a 100% chance.</param>
    public CrossoverStrategyRegistration<T> Rate(float crossoverRate)
    {
        if (crossoverRate < 0 || crossoverRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(crossoverRate), "Value must be between 0 and 1.");
        }

        _crossoverRate = crossoverRate;
        return this;
    }

    internal float GetCrossoverRate()
    {
        return _crossoverRate;
    }

    internal IList<BaseCrossoverStrategy<T>> GetRegisteredCrossoverStrategies()
    {
        return _crossoverStrategyConfig.CrossoverStrategies;
    }

    internal OperatorSelectionPolicy GetCrossoverSelectionPolicy()
    {
        return _crossoverSelectionPolicyConfig.Policy;
    }
}
