
namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// A simple operator selection policy that always selects the first available operator.
/// 
/// This policy provides a deterministic selection mechanism that consistently chooses
/// the first operator from the configured list. It's useful when you want predictable
/// behavior or when only one operator is available.
/// 
/// This policy is automatically applied when only a single crossover strategy is configured,
/// providing optimal performance for single-operator scenarios.
/// </summary>
public class FirstChoicePolicy : OperatorSelectionPolicy
{
    private IList<BaseOperator> _operators = [];

    /// <summary>
    /// Configures the policy with the available operators.
    /// </summary>
    /// <param name="operators">The list of operators to select from</param>
    /// <exception cref="ArgumentException">Thrown when no operators are provided</exception>
    public override void ApplyOperators(IList<BaseOperator> operators)
    {
        _operators = operators;
    }

    /// <summary>
    /// Selects the first operator from the configured list.
    /// </summary>
    /// <param name="random">Random number generator (not used by this policy)</param>
    /// <param name="epoch">Current epoch number of the genetic algorithm (not used by this policy)</param>
    /// <returns>The first operator in the list</returns>
    /// <exception cref="InvalidOperationException">Thrown when no operators are available for selection</exception>
    public override BaseOperator SelectOperator(Random random, int epoch)
    {
        if (_operators.Count == 0)
        {
            throw new InvalidOperationException("No operators available for selection.");
        }

        return _operators[0];
    }
}
