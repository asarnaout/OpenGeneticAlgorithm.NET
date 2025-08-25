namespace OpenGA.Net.OperatorSelectionPolicies;

public class OperatorSelectionPolicyConfiguration
{
    internal OperatorSelectionPolicy Policy = default!;

    public OperatorSelectionPolicy ApplyAdaptivePursuitPolicy(
        double learningRate = 0.1,
        double minimumProbability = 0.05,
        int rewardWindowSize = 10,
        double diversityWeight = 0.1,
        int minimumUsageBeforeAdaptation = 5)
    {
        if (learningRate < 0.0 || learningRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be between 0.0 and 1.0.");
        }

        if (minimumProbability < 0.0 || minimumProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumProbability), "Minimum probability must be between 0.0 and 1.0.");
        }

        var result = new AdaptivePursuitPolicy(
            learningRate,
            minimumProbability,
            rewardWindowSize,
            diversityWeight,
            minimumUsageBeforeAdaptation);

        Policy = result;
        return result;
    }

    public OperatorSelectionPolicy ApplyFirstChoicePolicy()
    {
        var result = new FirstChoicePolicy();
        Policy = result;
        return result;
    }
}
