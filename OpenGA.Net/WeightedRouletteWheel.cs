namespace OpenGA.Net;

internal struct WeightedRouletteWheel<T> where T : IEquatable<T>
{
    private Func<T, double> _weightSelector = default!;

    private IList<T> _candidates = [];

    private double[] _cumulativeProbabilities = [];

    private readonly Random _random = new();

    public WeightedRouletteWheel()
    {
    }

    internal static WeightedRouletteWheel<T> Init(IList<T> candidates, Func<T, double> weighBy)
    {
        if (candidates is not [_, ..])
        {
            throw new ArgumentException("Candidates list cannot be null or empty.", nameof(candidates));
        }

        ArgumentNullException.ThrowIfNull(weighBy);

        // Validate weights and detect edge cases
        var weights = candidates.Select(weighBy).ToArray();
        if (weights.Any(w => w < 0))
        {
            throw new ArgumentException("All weights must be non-negative.", nameof(weighBy));
        }

        if (weights.All(w => w == 0))
        {
            throw new ArgumentException("At least one weight must be greater than zero.", nameof(weighBy));
        }

        return new WeightedRouletteWheel<T>
        {
            _candidates = candidates,
            _weightSelector = weighBy
        }.SetupProbabilities(weighBy);
    }

    internal static WeightedRouletteWheel<T> InitWithUniformWeights(IList<T> candidates)
    {
        return Init(candidates, d => 1.0);
    }

    internal readonly T Spin()
    {
        if (_candidates is not [_, ..])
        {
            throw new InvalidOperationException("Cannot spin an empty roulette wheel.");
        }

        if (_candidates.Count == 1)
        {
            return _candidates[0];
        }

        var random = _random.NextDouble();

        // Binary search for O(log n) performance instead of O(n) linear search
        int left = 0, right = _cumulativeProbabilities.Length - 1;
        
        while (left < right)
        {
            int mid = (left + right) / 2;
            if (random <= _cumulativeProbabilities[mid])
            {
                right = mid;
            }
            else
            {
                left = mid + 1;
            }
        }

        return _candidates[left];
    }

    internal T SpinAndReadjustWheel()
    {
        var winner = Spin();

        var newCandidates = new List<T>(_candidates.Count - 1);
        foreach (var candidate in _candidates)
        {
            if (!candidate.Equals(winner))
            {
                newCandidates.Add(candidate);
            }
        }
        
        _candidates = newCandidates;

        if (_candidates.Count > 0)
        {
            SetupProbabilities(_weightSelector);
        }

        return winner;
    }

    private WeightedRouletteWheel<T> SetupProbabilities(Func<T, double> weightSelector)
    {
        if (_candidates.Count == 0)
        {
            _cumulativeProbabilities = [];
            return this;
        }

        if (_candidates.Count == 1)
        {
            _cumulativeProbabilities = [1.0];
            return this;
        }

        // Calculate weights once and cache them to avoid multiple function calls
        var weights = new double[_candidates.Count];
        var totalWeight = 0.0;
        
        for (int i = 0; i < _candidates.Count; i++)
        {
            weights[i] = weightSelector(_candidates[i]);
            totalWeight += weights[i];
        }
        
         _cumulativeProbabilities = new double[_candidates.Count];

        // Handle edge case where all remaining weights are zero (fallback to uniform)
        if (totalWeight == 0)
        {
            var uniformProbability = 1.0 / _candidates.Count;
            for (var i = 0; i < _candidates.Count; i++)
            {
                _cumulativeProbabilities[i] = (i + 1) * uniformProbability;
            }
            return this;
        }

        // Build cumulative probabilities directly without intermediate array
        var cumulativeSum = 0.0;

        for (var i = 0; i < _candidates.Count; i++)
        {
            var probability = weights[i] / totalWeight;
            cumulativeSum += probability;
            _cumulativeProbabilities[i] = cumulativeSum;
        }

        // Ensure the last probability is exactly 1.0 to handle floating point precision
        _cumulativeProbabilities[^1] = 1.0;

        return this;
    }
}
