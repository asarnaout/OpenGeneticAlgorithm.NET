using OpenGA.Net.CrossoverStrategies;

namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Implements Adaptive Pursuit algorithm for dynamic selection of crossover operators.
/// 
/// This algorithm maintains probability weights for each crossover operator and adapts them
/// based on their performance. Better-performing operators receive higher probabilities
/// over time, while ensuring minimum exploration for all operators.
/// </summary>
/// <typeparam name="T">The type of genes in the chromosome</typeparam>
public class AdaptivePursuitPolicy<T> : OperatorSelectionPolicy<T>
{
    private readonly Dictionary<BaseCrossoverStrategy<T>, double> _operatorProbabilities;
    private readonly Dictionary<BaseCrossoverStrategy<T>, double> _operatorRewards;
    private readonly Dictionary<BaseCrossoverStrategy<T>, int> _operatorUsageCount;
    private readonly Dictionary<BaseCrossoverStrategy<T>, Queue<double>> _recentRewards;
    
    private readonly double _learningRate;
    private readonly double _minimumProbability;
    private readonly int _rewardWindowSize;
    private readonly double _diversityWeight;
    private readonly int _minimumUsageBeforeAdaptation;
    
    /// <summary>
    /// Initializes a new instance of the AdaptivePursuitPolicy algorithm.
    /// </summary>
    /// <param name="operators">The list of crossover operators to adaptively select from</param>
    /// <param name="learningRate">Rate at which probabilities adapt (0.0 to 1.0, default: 0.1)</param>
    /// <param name="minimumProbability">Minimum probability for any operator to ensure exploration (default: 0.05)</param>
    /// <param name="rewardWindowSize">Number of recent rewards to consider for temporal weighting (default: 10)</param>
    /// <param name="diversityWeight">Weight given to diversity bonus in reward calculation (default: 0.1)</param>
    /// <param name="minimumUsageBeforeAdaptation">Minimum times each operator must be used before adaptation begins (default: 5)</param>
    public AdaptivePursuitPolicy(
        IList<BaseCrossoverStrategy<T>> operators,
        double learningRate = 0.1,
        double minimumProbability = 0.05,
        int rewardWindowSize = 10,
        double diversityWeight = 0.1,
        int minimumUsageBeforeAdaptation = 5)
    {
        if (operators is not { Count: > 0 })
        {
            throw new ArgumentException("At least one crossover operator must be provided.", nameof(operators));
        }

        if (learningRate < 0.0 || learningRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(learningRate), "Learning rate must be between 0.0 and 1.0.");
        }

        if (minimumProbability < 0.0 || minimumProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumProbability), "Minimum probability must be between 0.0 and 1.0.");
        }

        if (minimumProbability * operators.Count > 1.0)
        {
            throw new ArgumentException("Minimum probability times number of operators cannot exceed 1.0.");
        }
        
        _learningRate = learningRate;
        _minimumProbability = minimumProbability;
        _rewardWindowSize = rewardWindowSize;
        _diversityWeight = diversityWeight;
        _minimumUsageBeforeAdaptation = minimumUsageBeforeAdaptation;
        
        // Initialize equal probabilities for all operators
        var initialProbability = 1.0 / operators.Count;
        _operatorProbabilities = operators.ToDictionary(op => op, _ => initialProbability);
        _operatorRewards = operators.ToDictionary(op => op, _ => 0.0);
        _operatorUsageCount = operators.ToDictionary(op => op, _ => 0);
        _recentRewards = operators.ToDictionary(op => op, _ => new Queue<double>());
    }
    
    /// <summary>
    /// Selects a crossover operator based on current probabilities.
    /// </summary>
    /// <param name="random">Random number generator</param>
    /// <returns>The selected crossover operator</returns>
    public override BaseCrossoverStrategy<T> SelectOperator(Random random)
    {
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
    /// Updates the reward for a specific operator based on its performance.
    /// Step 1: Calculates the reward as the improvement in fitness by subtracting the best parent's fitness from the best offspring's fitness.
    /// Step 2: Normalizes the reward by the population's fitness range.
    /// Step 3: Adds a diversity bonus if applicable to get the 'total reward'
    /// Step 4: Adds the 'total reward' to the recent rewards queue for that specific operator. If the queue grows beyond a max window size, then the oldest reward is removed.
    /// Step 5: Uses the recent rewards queue for that specific operator to calculate a weighted average reward. This weighted average emphasizes recent weights more than older ones.
    /// Step 6: Updates the probabilities if the operator has been used a sufficient number of times.
    /// </summary>
    /// <param name="crossoverOperator">The crossover operator that was used</param>
    /// <param name="bestParentFitness">Fitness of the best parent before crossover</param>
    /// <param name="bestOffspringFitness">Fitness of the best offspring after crossover</param>
    /// <param name="populationFitnessRange">Current fitness range in the population for normalization</param>
    /// <param name="offspringDiversity">Measure of diversity among the offspring (optional)</param>
    public override void UpdateReward(
        BaseCrossoverStrategy<T> crossoverOperator,
        double bestParentFitness,
        double bestOffspringFitness,
        double populationFitnessRange,
        double offspringDiversity = 0.0)
    {
        if (!_operatorProbabilities.ContainsKey(crossoverOperator))
        {
            throw new ArgumentException("Unknown crossover operator.", nameof(crossoverOperator));
        }
        
        // Calculate primary fitness improvement reward
        var fitnessImprovement = bestOffspringFitness - bestParentFitness;
        
        // Normalize by population fitness range to handle different problem scales
        var normalizedReward = populationFitnessRange > 0 
            ? fitnessImprovement / populationFitnessRange 
            : fitnessImprovement;
        
        // Add diversity bonus to reward operators that generate more diverse offspring
        // This helps prevent premature convergence by encouraging exploration of different
        // genetic combinations, maintaining population diversity which is crucial for finding
        // global optima rather than getting stuck in local optima
        var diversityBonus = _diversityWeight * offspringDiversity;
        var totalReward = normalizedReward + diversityBonus;
        
        // Update recent rewards queue
        var recentQueue = _recentRewards[crossoverOperator];
        recentQueue.Enqueue(totalReward);
        if (recentQueue.Count > _rewardWindowSize)
        {
            recentQueue.Dequeue();
        }
        
        // Calculate weighted average with more weight on recent rewards
        var weightedReward = CalculateWeightedReward(recentQueue);
        _operatorRewards[crossoverOperator] = weightedReward;

        // Update probabilities if minimum usage threshold is met
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
        var newProbabilities = new Dictionary<BaseCrossoverStrategy<T>, double>();
        
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
    private void NormalizeProbabilities(Dictionary<BaseCrossoverStrategy<T>, double> probabilities)
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
        var weights = new Dictionary<BaseCrossoverStrategy<T>, double>(n);
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
