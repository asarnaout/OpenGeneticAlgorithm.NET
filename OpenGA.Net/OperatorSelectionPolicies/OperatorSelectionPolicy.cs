using OpenGA.Net.CrossoverStrategies;

namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Abstract base class for operator selection policies that adaptively choose genetic operators.
/// </summary>
public abstract class OperatorSelectionPolicy
{
    /// <summary>
    /// Selects an operator based on the policy's selection mechanism.
    /// </summary>
    /// <param name="random">Random number generator</param>
    /// <returns>The selected operator</returns>
    public abstract BaseOperator SelectOperator(Random random);

    public abstract void ApplyOperators(IList<BaseOperator> operators);
}
