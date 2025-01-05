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

    internal T Spin()
    {
        var random = _random.NextDouble();

        for (int i = 0; i < _cumulativeProbabilities.Length; i++)
        {
            if (random <= _cumulativeProbabilities[i])
            {
                return _candidates[i];
            }
        }

        return _candidates[^1];
    }

    internal T SpinAndReadjustWheel()
    {
        var winner = Spin();

        _candidates = _candidates.Where(x => !x.Equals(winner)).ToList();

        SetupProbabilities(_weightSelector);

        return winner;
    }

    private WeightedRouletteWheel<T> SetupProbabilities(Func<T, double> weightSelector)
    {
        if (_candidates.Count == 0)
        {
            return this;
        }

        var totalWeight = _candidates.Sum(x => weightSelector(x));

        var probabilities = _candidates
            .Select(candidate => weightSelector(candidate) / totalWeight)
            .ToArray();

        _cumulativeProbabilities = new double[_candidates.Count];

        var cumulativeSum = 0d;

        for (var i = 0; i < _candidates.Count; i++)
        {
            cumulativeSum += probabilities[i];
            _cumulativeProbabilities[i] = cumulativeSum;
        }

        return this;
    }
}
