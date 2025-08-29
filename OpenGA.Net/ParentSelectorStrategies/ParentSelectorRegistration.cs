using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.ParentSelectorStrategies;

/// <summary>
/// Provides configuration and registration capabilities for parent selector strategies in the genetic algorithm.
/// This class manages the registration of parent selector strategies and operator selection policies.
/// 
/// The registration process supports both single and multiple parent selector strategies with intelligent
/// defaults applied by OpenGARunner when strategies are not explicitly configured.
/// </summary>
/// <typeparam name="T">The type of gene values contained within chromosomes</typeparam>
public class ParentSelectorRegistration<T>
{
    private readonly ParentSelectorConfiguration<T> _singleParentSelectorConfig = new();

    private readonly MultiParentSelectorConfiguration<T> _multiParentSelectorConfig = new();

    private bool _isMultiRegistration = false;

    /// <summary>
    /// Registers a single parent selector strategy for use in the genetic algorithm.
    /// 
    /// This method is intended for scenarios where only one parent selector strategy is needed.
    /// 
    /// If no parent selector strategies are registered at all, OpenGARunner defaults to Tournament selection
    /// using this registration method during the DefaultMissingStrategies() process.
    /// </summary>
    /// <param name="singleRegistration">
    /// A configuration action that registers exactly one parent selector strategy.
    /// Examples: s => s.Tournament(), s => s.RouletteWheel()
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when singleRegistration is null</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no parent selector strategy is found at the last step of this method's execution.
    /// </exception>
    /// <example>
    /// <code>
    /// .ParentSelection(p => p.RegisterSingle(s => s.Tournament()))
    /// </code>
    /// </example>
    public ParentSelectorRegistration<T> RegisterSingle(Action<ParentSelectorConfiguration<T>> singleRegistration)
    {
        ArgumentNullException.ThrowIfNull(singleRegistration, nameof(singleRegistration));

        singleRegistration(_singleParentSelectorConfig);

        if (_singleParentSelectorConfig.ParentSelector is null)
        {
            throw new InvalidOperationException("No parent selector strategy was registered.");
        }

        _isMultiRegistration = false;

        return this;
    }

    /// <summary>
    /// Registers multiple parent selector strategies for use in the genetic algorithm.
    /// 
    /// This method enables the registration of multiple parent selector strategies that will be
    /// selected between during algorithm execution. When multiple strategies are registered,
    /// OpenGARunner applies intelligent operator selection policy defaults:
    /// 
    /// 1. If any strategy has custom weights (> 0), CustomWeightPolicy is automatically applied
    /// 2. If no custom weights and no explicit policy, AdaptivePursuitPolicy is applied by default
    /// 3. If an explicit policy is configured that conflicts with custom weights, an exception is thrown
    /// 
    /// The operator selection policy determines how the algorithm chooses between the registered
    /// parent selector strategies during each parent selection cycle.
    /// </summary>
    /// <param name="configurator">
    /// A configuration action that registers multiple parent selector strategies.
    /// Can include custom weights and strategy-specific configurations.
    /// </param>
    /// <returns>
    /// The ParentSelectorRegistration instance for method chaining, allowing
    /// further configuration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when configurator is null</exception>
    /// <example>
    /// <code>
    /// .ParentSelection(p => p.RegisterMulti(m => m
    ///     .Tournament(customWeight: 0.6f)
    ///     .RouletteWheel(customWeight: 0.4f)
    ///     .WithPolicy(p => p.AdaptivePursuit())
    /// ))
    /// </code>
    /// </example>
    public ParentSelectorRegistration<T> RegisterMulti(Action<MultiParentSelectorConfiguration<T>> configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        configurator(_multiParentSelectorConfig);
        _isMultiRegistration = true;

        return this;
    }

    internal void ValidateAndDefault(Random random)
    {
        if (_isMultiRegistration)
        {
            _multiParentSelectorConfig.ValidateAndDefault(random);
        }
        else
        {
            _singleParentSelectorConfig.ValidateAndDefault(random);
        }
    }

    internal OperatorSelectionPolicy GetParentSelectorSelectionPolicy()
    {
        return _isMultiRegistration
            ? _multiParentSelectorConfig.GetParentSelectorSelectionPolicy()
            : _singleParentSelectorConfig.GetParentSelectorSelectionPolicy();
    }
}
