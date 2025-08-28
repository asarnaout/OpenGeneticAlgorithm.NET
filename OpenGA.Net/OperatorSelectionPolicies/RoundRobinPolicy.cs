namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// A round-robin operator selection policy that cycles through available operators in order.
/// 
/// This policy provides a deterministic selection mechanism that ensures fair distribution
/// of operator usage by cycling through all available operators sequentially. Each operator
/// is selected an equal number of times over the course of the algorithm's execution.
/// 
/// This policy is useful when you want to ensure balanced usage of all operators without
/// any bias towards specific operators, providing predictable rotation behavior.
/// </summary>
public class RoundRobinPolicy : OperatorSelectionPolicy
{
    private int _currentIndex = 0;

    /// <summary>
    /// Selects the next operator in the round-robin sequence.
    /// 
    /// This method cycles through the available operators in order, wrapping back
    /// to the first operator after reaching the end of the list.
    /// </summary>
    /// <param name="random">Random number generator (not used by this policy)</param>
    /// <param name="epoch">Current epoch number of the genetic algorithm (not used by this policy)</param>
    /// <returns>The next operator in the round-robin sequence</returns>
    /// <exception cref="InvalidOperationException">Thrown when no operators are available for selection</exception>
    public override BaseOperator SelectOperator(Random random, int epoch)
    {
        if (Operators.Count == 0)
        {
            throw new InvalidOperationException("No operators available for selection.");
        }

        var selectedOperator = Operators[_currentIndex];
        _currentIndex = (_currentIndex + 1) % Operators.Count;
        
        return selectedOperator;
    }
}
