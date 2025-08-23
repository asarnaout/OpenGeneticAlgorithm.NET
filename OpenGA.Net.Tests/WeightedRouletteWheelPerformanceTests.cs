using OpenGA.Net;

namespace OpenGA.Net.Tests;

public class WeightedRouletteWheelPerformanceTests
{
    [Fact]
    public void Init_WithNullCandidates_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            WeightedRouletteWheel<int>.Init(null!, x => 1.0));
    }

    [Fact]
    public void Init_WithEmptyCandidates_ShouldThrowArgumentException()
    {
        var emptyCandidates = new List<int>();
        
        Assert.Throws<ArgumentException>(() => 
            WeightedRouletteWheel<int>.Init(emptyCandidates, x => 1.0));
    }

    [Fact]
    public void Init_WithNullWeightSelector_ShouldThrowArgumentNullException()
    {
        var candidates = new List<int> { 1, 2, 3 };
        
        Assert.Throws<ArgumentNullException>(() => 
            WeightedRouletteWheel<int>.Init(candidates, null!));
    }

    [Fact]
    public void Init_WithNegativeWeights_ShouldThrowArgumentException()
    {
        var candidates = new List<int> { 1, 2, 3 };
        
        Assert.Throws<ArgumentException>(() => 
            WeightedRouletteWheel<int>.Init(candidates, x => -1.0));
    }

    [Fact]
    public void Init_WithAllZeroWeights_ShouldThrowArgumentException()
    {
        var candidates = new List<int> { 1, 2, 3 };
        
        Assert.Throws<ArgumentException>(() => 
            WeightedRouletteWheel<int>.Init(candidates, x => 0.0));
    }

    [Fact]
    public void Spin_WithSingleCandidate_ShouldReturnThatCandidate()
    {
        var candidates = new List<int> { 42 };
        var wheel = WeightedRouletteWheel<int>.Init(candidates, x => 1.0);
        
        var result = wheel.Spin();
        
        Assert.Equal(42, result);
    }

    [Fact]
    public void Spin_WithEmptyWheel_ShouldThrowInvalidOperationException()
    {
        var candidates = new List<int> { 1 };
        var wheel = WeightedRouletteWheel<int>.Init(candidates, x => 1.0);
        
        // Remove the only candidate
        wheel.SpinAndReadjustWheel();
        
        Assert.Throws<InvalidOperationException>(() => wheel.Spin());
    }

    [Fact]
    public void BinarySearch_PerformanceTest_ShouldBeEfficient()
    {
        // Create a large population to test binary search performance
        var candidates = Enumerable.Range(1, 10000).ToList();
        var wheel = WeightedRouletteWheel<int>.InitWithUniformWeights(candidates);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Perform many spins
        for (int i = 0; i < 1000; i++)
        {
            wheel.Spin();
        }
        
        stopwatch.Stop();
        
        // Should complete quickly (less than 1 second for 1000 spins on 10k candidates)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Performance test took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public void WeightedDistribution_ShouldFavorHigherWeights()
    {
        var candidates = new List<int> { 1, 2, 3 };
        // Give candidate 3 much higher weight (100x more)
        var wheel = WeightedRouletteWheel<int>.Init(candidates, x => x == 3 ? 100.0 : 1.0);
        
        var counts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
        
        // Spin many times and count results
        for (int i = 0; i < 10000; i++)
        {
            var result = wheel.Spin();
            counts[result]++;
        }
        
        // Candidate 3 should be selected much more often
        Assert.True(counts[3] > counts[1] * 10); // At least 10x more often
        Assert.True(counts[3] > counts[2] * 10); // At least 10x more often
    }

    [Fact]
    public void SpinAndReadjustWheel_ShouldRemoveSelectedCandidate()
    {
        var candidates = new List<int> { 1, 2, 3 };
        var wheel = WeightedRouletteWheel<int>.InitWithUniformWeights(candidates);
        
        var firstWinner = wheel.SpinAndReadjustWheel();
        var secondWinner = wheel.SpinAndReadjustWheel();
        var thirdWinner = wheel.SpinAndReadjustWheel();
        
        // All three should be different
        var winners = new[] { firstWinner, secondWinner, thirdWinner };
        Assert.Equal(3, winners.Distinct().Count());
        
        // All original candidates should be represented
        Assert.True(candidates.All(c => winners.Contains(c)));
    }

    [Fact]
    public void UniformWeights_ShouldEventuallySelectAllCandidates()
    {
        var candidates = new List<int> { 1, 2, 3, 4, 5 };
        var wheel = WeightedRouletteWheel<int>.InitWithUniformWeights(candidates);
        
        var selectedCandidates = new HashSet<int>();
        
        // Spin many times to ensure all candidates are eventually selected
        for (int i = 0; i < 1000; i++)
        {
            selectedCandidates.Add(wheel.Spin());
            if (selectedCandidates.Count == candidates.Count)
                break;
        }
        
        Assert.Equal(candidates.Count, selectedCandidates.Count);
        Assert.True(candidates.All(c => selectedCandidates.Contains(c)));
    }

    [Fact]
    public void ZeroWeightsInSubset_ShouldFallbackToUniform()
    {
        var candidates = new List<int> { 1, 2, 3 };
        var wheel = WeightedRouletteWheel<int>.Init(candidates, x => x == 1 ? 1.0 : 0.0);
        
        // Spin and remove the only weighted candidate
        wheel.SpinAndReadjustWheel(); // This should remove candidate 1
        
        // Now try to spin - should work with uniform distribution
        var result = wheel.Spin();
        Assert.True(result == 2 || result == 3);
    }

    [Fact]
    public void FloatingPointPrecision_ShouldBeHandledCorrectly()
    {
        var candidates = new List<double> { 0.1, 0.2, 0.3 };
        var wheel = WeightedRouletteWheel<double>.Init(candidates, x => x);
        
        // Should not throw due to floating point precision issues
        for (int i = 0; i < 1000; i++)
        {
            var result = wheel.Spin();
            Assert.Contains(result, candidates);
        }
    }
}
