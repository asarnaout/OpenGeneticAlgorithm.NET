namespace OpenGA.Net;

public class WeightedRouletteWheel<T> where T : IEquatable<T>
{
    private Func<T, double> _weightSelector = default!;

    private IList<T> _candidates = [];

    private double[] _cumulativeProbabilities = [];

    private readonly Random _random = new();

    private WeightedRouletteWheel()
    {
    }

    public static WeightedRouletteWheel<T> Init(IList<T> candidates, Func<T, double> weighBy)
    {
        return new WeightedRouletteWheel<T>
        {
            _candidates = candidates,
            _weightSelector = weighBy
        }.SetupProbabilities();
    }

    public static WeightedRouletteWheel<T> InitWithUniformWeights(IList<T> candidates)
    {
        return new WeightedRouletteWheel<T>
        {
            _candidates = candidates,
            _weightSelector = d => 1.0
        }.SetupProbabilities();
    }

    public T Spin()
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

    public T SpinAndReadjustWheel()
    {
        var winner = Spin();

        _candidates = _candidates.Where(x => !x.Equals(winner)).ToList();

        SetupProbabilities();

        return winner;
    }

    private WeightedRouletteWheel<T> SetupProbabilities()
    {
        if (_candidates.Count == 0)
        {
            return this;
        }

        var totalWeight = _candidates.Sum(x => _weightSelector(x));

        var probabilities = _candidates
            .Select(candidate => _weightSelector(candidate) / totalWeight)
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
