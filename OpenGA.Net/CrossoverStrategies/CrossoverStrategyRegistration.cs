using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.CrossoverStrategies;

public class CrossoverStrategyRegistration<T>
{
    private readonly CrossoverStrategyConfiguration<T> _crossoverStrategyConfig = new();

    private readonly OperatorSelectionPolicyConfiguration _crossoverSelectionPolicyConfig = new();

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

    internal IList<BaseCrossoverStrategy<T>> GetRegisteredCrossoverStrategies()
    {
        return _crossoverStrategyConfig.CrossoverStrategies;
    }

    internal OperatorSelectionPolicy GetCrossoverSelectionPolicy()
    {
        return _crossoverSelectionPolicyConfig.Policy;
    }
}
