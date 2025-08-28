namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Implements Adaptive Pursuit algorithm for dynamic selection of genetic operators.
/// 
/// This algorithm maintains probability weights for each operator and adapts them
/// based on their performance. Better-performing operators receive higher probabilities
/// over time, while ensuring minimum exploration for all operators.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AdaptivePursuitPolicy algorithm.
/// </remarks>
/// <param name="learningRate">Rate at which probabilities adapt (0.0 to 1.0, default: 0.1)</param>
/// <param name="minimumProbability">Minimum probability for any operator to ensure exploration (default: 0.05)</param>
/// <param name="rewardWindowSize">Number of recent rewards to consider for temporal weighting (default: 10)</param>
/// <param name="diversityWeight">Weight given to diversity bonus in reward calculation (default: 0.1)</param>
/// <param name="minimumUsageBeforeAdaptation">Minimum times each operator must be used before adaptation begins (default: 5)</param>
/// <param name="warmupRuns">Number of warm-up runs before adaptation begins (default: 10)</param>
public class AdaptivePursuitPolicy(
    double learningRate,
    double minimumProbability,
    int rewardWindowSize,
    double diversityWeight,
    int minimumUsageBeforeAdaptation,
    int warmupRuns) : OperatorSelectionPolicy
{
    private Dictionary<BaseOperator, double> _operatorProbabilities = [];
    private Dictionary<BaseOperator, double> _operatorRewards = [];
    private Dictionary<BaseOperator, int> _operatorUsageCount = [];
    private Dictionary<BaseOperator, Queue<double>> _recentRewards = [];
    
    private readonly double _learningRate = learningRate;
    private readonly double _minimumProbability = minimumProbability;
    private readonly int _rewardWindowSize = rewardWindowSize;
    private readonly double _diversityWeight = diversityWeight;
    private readonly int _minimumUsageBeforeAdaptation = minimumUsageBeforeAdaptation;
    private readonly int _warmupRuns = warmupRuns;
    private int _roundRobinIndex = 0;

    protected internal override void ApplyOperators(IList<BaseOperator> operators)
    {
        base.ApplyOperators(operators);

        if (_minimumProbability * operators.Count > 1.0)
        {
            throw new ArgumentException("Minimum probability times number of operators cannot exceed 1.0.");
        }

        var initialProbability = 1.0 / operators.Count;
        _operatorProbabilities = operators.ToDictionary(op => op, _ => initialProbability);
        _operatorRewards = operators.ToDictionary(op => op, _ => 0.0);
        _operatorUsageCount = operators.ToDictionary(op => op, _ => 0);
        _recentRewards = operators.ToDictionary(op => op, _ => new Queue<double>());
        
        // Reset round-robin index when operators are applied
        _roundRobinIndex = 0;
    }
    
    /// <summary>
    /// Selects an operator based on current probabilities.
    /// During warmup period (epoch <= warmupRuns), uses round-robin selection to ensure equal initial usage.
    /// After warmup, uses adaptive probability-based selection.
    /// </summary>
    /// <param name="random">Random number generator</param>
    /// <param name="epoch">Current epoch number of the genetic algorithm</param>
    /// <returns>The selected operator</returns>
    public override BaseOperator SelectOperator(Random random, int epoch)
    {
        // During warmup period, use round-robin selection to ensure fair initial usage
        if (epoch <= _warmupRuns)
        {
            var selectedOperator = Operators[_roundRobinIndex];
            _roundRobinIndex = (_roundRobinIndex + 1) % Operators.Count;
            _operatorUsageCount[selectedOperator]++;
            return selectedOperator;
        }

        // After warmup, use adaptive probability-based selection
        var randomValue = random.NextDouble();
        var cumulativeProbability = 0.0;

        foreach (var kvp in _operatorProbabilities)
        {
            cumulativeProbability += kvp.Value;
            if (randomValue <= cumulativeProbability)
            {
                _operatorUsageCount[kvp.Key]++;
                return kvp.Key;
            }
        }

        // Fallback (should not happen with proper probabilities)
        var fallbackOperator = _operatorProbabilities.Keys.First();
        _operatorUsageCount[fallbackOperator]++;
        return fallbackOperator;
    }
    
    /// <summary>
    /// Updates the reward for a specific operator based on its observed effect.
    /// Steps:
    /// 1) Compute fitness improvement as (postFitnessMetric - preFitnessMetric)
    /// 2) Normalize by a problem-scale factor (normalizationRange)
    /// 3) Add a diversity component (diversitySignal) to encourage exploration
    /// 4) Push into a recent-rewards window and compute a recency-weighted average
    /// 5) Adapt operator probabilities when minimum usage thresholds are met
    /// </summary>
    /// <param name="appliedOperator">The operator that was applied</param>
    /// <param name="preFitnessMetric">Fitness metric before applying the operator</param>
    /// <param name="postFitnessMetric">Fitness metric after applying the operator</param>
    /// <param name="normalizationRange">Scale used to normalize fitness improvement (e.g., population fitness range)</param>
    /// <param name="diversitySignal">Auxiliary diversity signal (e.g., offspring std-dev or diversity delta)</param>
    public void UpdateReward(
        BaseOperator appliedOperator,
        double preFitnessMetric,
        double postFitnessMetric,
        double normalizationRange,
        double diversitySignal)
    {
        if (!_operatorProbabilities.ContainsKey(appliedOperator))
        {
            throw new ArgumentException("Unknown operator.", nameof(appliedOperator));
        }
        
        // Calculate primary fitness improvement reward
        var fitnessImprovement = postFitnessMetric - preFitnessMetric;
        
        // Normalize by population fitness range to handle different problem scales
        var normalizedReward = normalizationRange > 0 
            ? fitnessImprovement / normalizationRange 
            : fitnessImprovement;

        /*
         * Add diversity bonus to reward operators that generate more diverse results
         * This helps prevent premature convergence by encouraging exploration of different
         * genetic combinations, maintaining population diversity which is crucial for finding
         * global optima rather than getting stuck in local optima
         */
        var diversityBonus = _diversityWeight * diversitySignal;
        var totalReward = normalizedReward + diversityBonus;
        
        var recentQueue = _recentRewards[appliedOperator];
        recentQueue.Enqueue(totalReward);
        if (recentQueue.Count > _rewardWindowSize)
        {
            recentQueue.Dequeue();
        }
        
        var weightedReward = CalculateWeightedReward(recentQueue);
        _operatorRewards[appliedOperator] = weightedReward;

        /*
         * Update probabilities if minimum usage threshold is met. We update probabilities only if all
         * all operators have been sufficiently used so we do not make decisions based on insufficient data.
         */
        var shouldAdapt = _operatorUsageCount.Values.All(count => count >= _minimumUsageBeforeAdaptation);
        if (shouldAdapt)
        {
            UpdateProbabilities();
        }
    }
    
    /// <summary>
    /// Calculates a weighted average of recent rewards, giving more weight to recent performance.
    /// </summary>
    /// <param name="recentQueue">Queue of recent rewards</param>
    /// <returns>Weighted average reward</returns>
    private static double CalculateWeightedReward(Queue<double> recentQueue)
    {
        if (recentQueue.Count == 0)
        {
            return 0.0;
        }
        
        var rewards = recentQueue.ToArray();
        var totalWeight = 0.0;
        var weightedSum = 0.0;
        
        for (int i = 0; i < rewards.Length; i++)
        {
            // More recent rewards get higher weight (exponential decay)
            var weight = Math.Exp(-(rewards.Length - 1 - i) * 0.1);
            weightedSum += rewards[i] * weight;
            totalWeight += weight;
        }
        
        return totalWeight > 0 ? weightedSum / totalWeight : 0.0;
    }
    
    /// <summary>
    /// Updates operator probabilities based on their performance using the Adaptive Pursuit algorithm.
    /// </summary>
    private void UpdateProbabilities()
    {
        // Find the best performing operator
        var bestOperator = _operatorRewards.OrderByDescending(kvp => kvp.Value).First().Key;
        
        // Update probabilities using Adaptive Pursuit formula
        var newProbabilities = new Dictionary<BaseOperator, double>();
        
        foreach (var kvp in _operatorProbabilities)
        {
            var currentProb = kvp.Value;
            
            if (kvp.Key == bestOperator)
            {
                // Increase probability for best operator
                var newProb = currentProb + _learningRate * (1.0 - currentProb);
                newProbabilities[kvp.Key] = newProb;
            }
            else
            {
                // Decrease probability for other operators
                var newProb = currentProb - _learningRate * currentProb;
                newProbabilities[kvp.Key] = Math.Max(newProb, _minimumProbability);
            }
        }
        
        // Normalize probabilities to ensure they sum to 1.0
        NormalizeProbabilities(newProbabilities);
        
        // Update the stored probabilities
        foreach (var kvp in newProbabilities)
        {
            _operatorProbabilities[kvp.Key] = kvp.Value;
        }
    }
    
    /// <summary>
    /// Normalizes probabilities to ensure they sum to 1.0 while respecting minimum probability constraints.
    /// </summary>
    /// <param name="probabilities">Dictionary of probabilities to normalize</param>
    private void NormalizeProbabilities(Dictionary<BaseOperator, double> probabilities)
    {
        // Enforce a minimum probability for each operator while making sure the
        // probabilities sum to 1.0. Approach:
        // 1. Clamp each probability to at least _minimumProbability.
        // 2. Compute remaining mass = 1 - (n * _minimumProbability).
        // 3. Distribute remaining mass proportionally to the (prob - min) values
        //    for operators that are above the minimum. If none are above minimum,
        //    distribute remaining mass equally.
        var keys = probabilities.Keys.ToList();
        var n = keys.Count;

        if (n == 0)
        {
            return;
        }

        // Step 1: clamp to minimum
        foreach (var key in keys)
        {
            probabilities[key] = Math.Max(probabilities[key], _minimumProbability);
        }

        // Remaining mass to distribute above the minimums
        var remainingMass = 1.0 - _minimumProbability * n;

        // If remaining mass is non-positive, fall back to equal probabilities
        if (remainingMass <= 0.0)
        {
            var equalProb = 1.0 / n;
            foreach (var key in keys)
            {
                probabilities[key] = equalProb;
            }
            return;
        }

        // Compute weights for redistribution: how much each entry is above the min
        var weights = new Dictionary<BaseOperator, double>(n);
        var totalWeight = 0.0;

        foreach (var key in keys)
        {
            var w = Math.Max(probabilities[key] - _minimumProbability, 0.0);
            weights[key] = w;
            totalWeight += w;
        }

        if (totalWeight <= 0.0)
        {
            // Everyone is exactly at the minimum; distribute remainingMass equally.
            var extra = remainingMass / n;
            foreach (var key in keys)
            {
                probabilities[key] = _minimumProbability + extra;
            }
            return;
        }

        // Distribute remaining mass proportionally to weights
        foreach (var key in keys)
        {
            var extra = weights[key] / totalWeight * remainingMass;
            probabilities[key] = _minimumProbability + extra;
        }

        // Numerical safety: ensure sum is exactly 1.0 by a tiny correction on the largest entry
        var sumAfter = probabilities.Values.Sum();
        var diff = 1.0 - sumAfter;
        if (Math.Abs(diff) > 1e-12)
        {
            // Apply correction to the entry with the largest probability
            var maxKey = probabilities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            probabilities[maxKey] += diff;
            // Ensure correction did not push below minimum (very unlikely)
            if (probabilities[maxKey] < _minimumProbability)
            {
                probabilities[maxKey] = _minimumProbability;
            }
        }
    }
}
