namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// A weighted operator selection policy that selects operators based on their custom weights.
/// 
/// This policy uses the CustomWeight property of each operator to determine selection probability.
/// Operators with higher weights have a greater chance of being selected. The weights are 
/// normalized automatically, so they don't need to sum to 1.0.
/// 
/// The selection is performed using a weighted roulette wheel algorithm for efficient 
/// probability-based selection. If all operators have zero weight, uniform selection is applied.
/// </summary>
public class CustomWeightPolicy : OperatorSelectionPolicy
{
    private WeightedRouletteWheel<BaseOperator> _rouletteWheel;

    /// <summary>
    /// Configures the policy with the available operators.
    /// </summary>
    /// <param name="operators">The list of operators to select from based on their custom weights</param>
    /// <param name="random">Random number generator for policies that require randomization during initialization</param>
    /// <exception cref="ArgumentException">Thrown when no operators are provided</exception>
    protected internal override void ApplyOperators(IList<BaseOperator> operators, Random random)
    {
        base.ApplyOperators(operators, random);

        // Check if all weights are zero - if so, use uniform weights
        var hasNonZeroWeights = operators.Any(op => op.CustomWeight > 0);
        
        if (hasNonZeroWeights)
        {
            _rouletteWheel = WeightedRouletteWheel<BaseOperator>.Init(operators, op => op.CustomWeight);
        }
        else
        {
            _rouletteWheel = WeightedRouletteWheel<BaseOperator>.InitWithUniformWeights(operators);
        }
    }

    /// <summary>
    /// Selects an operator based on the custom weights using weighted roulette wheel selection.
    /// 
    /// Operators with higher custom weights have a proportionally higher probability of being selected.
    /// The weights are automatically normalized, so they don't need to sum to 1.0.
    /// </summary>
    /// <param name="random">Random number generator used for selection (not used since WeightedRouletteWheel has its own)</param>
    /// <param name="epoch">Current epoch number of the genetic algorithm (not used by this policy)</param>
    /// <returns>An operator selected based on weighted probability</returns>
    /// <exception cref="InvalidOperationException">Thrown when no operators are available for selection</exception>
    public override BaseOperator SelectOperator(Random random, int epoch)
    {
        try
        {
            return _rouletteWheel.Spin();
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("No operators available for selection.");
        }
    }
}
