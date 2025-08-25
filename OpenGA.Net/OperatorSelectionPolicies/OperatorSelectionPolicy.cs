namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Abstract base class for operator selection policies that adaptively choose genetic operators.
/// 
/// Operator selection policies determine which genetic operator to use during the evolution process
/// when multiple operators are available. Different policies implement various strategies for selection,
/// such as adaptive learning based on performance feedback, random selection, or deterministic approaches.
/// 
/// Common implementations include:
/// - AdaptivePursuitPolicy: Learns which operators perform best and increases their selection probability
/// - FirstChoicePolicy: Always selects the first available operator for predictable behavior
/// </summary>
public abstract class OperatorSelectionPolicy
{
    /// <summary>
    /// Selects an operator based on the policy's selection mechanism.
    /// </summary>
    /// <param name="random">Random number generator for policies that require randomization</param>
    /// <returns>The selected operator according to the policy's strategy</returns>
    public abstract BaseOperator SelectOperator(Random random);

    /// <summary>
    /// Configures the policy with the available operators that can be selected from.
    /// 
    /// This method is called during genetic algorithm initialization to provide the policy
    /// with the list of operators it can choose from during evolution.
    /// </summary>
    /// <param name="operators">The list of available operators for selection</param>
    public abstract void ApplyOperators(IList<BaseOperator> operators);
}
