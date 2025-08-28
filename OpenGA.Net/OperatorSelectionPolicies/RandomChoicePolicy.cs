namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// A random choice operator selection policy that randomly selects from available operators.
/// 
/// This policy provides a stochastic selection mechanism that randomly chooses operators
/// with equal probability. Unlike deterministic policies, this introduces randomness into
/// operator selection, which can help avoid systematic biases and provide more natural
/// exploration patterns.
/// 
/// Over many selections, this policy will tend toward equal usage of all operators
/// (statistical fairness) but does not guarantee perfect balance like round-robin.
/// </summary>
public class RandomChoicePolicy : OperatorSelectionPolicy
{
    /// <summary>
    /// Randomly selects an operator from the available operators.
    /// 
    /// Each operator has an equal probability of being selected on each call.
    /// The selection is independent of previous selections, providing true
    /// random choice behavior.
    /// </summary>
    /// <param name="random">Random number generator used for selection</param>
    /// <param name="epoch">Current epoch number of the genetic algorithm (not used by this policy)</param>
    /// <returns>A randomly selected operator</returns>
    /// <exception cref="InvalidOperationException">Thrown when no operators are available for selection</exception>
    /// <exception cref="ArgumentNullException">Thrown when random parameter is null</exception>
    public override BaseOperator SelectOperator(Random random, int epoch)
    {
        if (Operators.Count == 0)
        {
            throw new InvalidOperationException("No operators available for selection.");
        }

        if (random is null)
        {
            throw new ArgumentNullException(nameof(random), "Random number generator cannot be null.");
        }

        var randomIndex = random.Next(Operators.Count);
        return Operators[randomIndex];
    }
}
