using OpenGA.Net.Termination;
using OpenGA.Net.ParentSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;
using System.Diagnostics;

namespace OpenGA.Net.Tests.Termination;

public class TargetStandardDeviationTerminationStrategyTests
{
    private GeneticAlgorithmState CreateMockStateWithFitness(double highestFitness)
    {
        var stopwatch = new Stopwatch();
        return new GeneticAlgorithmState(0, stopwatch, highestFitness);
    }

    [Fact]
    public void Constructor_WithNegativeStandardDeviation_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new TargetStandardDeviationTerminationStrategy<int>(-0.1));
    }

    [Fact]
    public void Constructor_WithValidStandardDeviation_CreatesInstance()
    {
        // Act
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1);

        // Assert
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Terminate_WithLessThanTwoValues_ReturnsFalse()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1);
        var state = CreateMockStateWithFitness(5.0);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WithTwoIdenticalValues_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1);
        var state = CreateMockStateWithFitness(5.0);

        // Act - call twice with same fitness
        strategy.Terminate(state);
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithHighVariation_ReturnsFalse()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1);

        // Act - call with varying fitness values
        strategy.Terminate(CreateMockStateWithFitness(1.0));
        strategy.Terminate(CreateMockStateWithFitness(5.0));
        strategy.Terminate(CreateMockStateWithFitness(10.0));
        var result = strategy.Terminate(CreateMockStateWithFitness(15.0));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WithLowVariationBelowThreshold_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(1.0);

        // Act - call with similar fitness values
        strategy.Terminate(CreateMockStateWithFitness(5.0));
        strategy.Terminate(CreateMockStateWithFitness(5.1));
        strategy.Terminate(CreateMockStateWithFitness(5.2));
        var result = strategy.Terminate(CreateMockStateWithFitness(5.1));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithCustomWindow_TracksCorrectNumberOfValues()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1, window: 3);

        // Act - call more times than window size
        strategy.Terminate(CreateMockStateWithFitness(1.0));
        strategy.Terminate(CreateMockStateWithFitness(2.0));
        strategy.Terminate(CreateMockStateWithFitness(3.0));
        strategy.Terminate(CreateMockStateWithFitness(3.1)); // Should drop the first value (1.0)
        var result = strategy.Terminate(CreateMockStateWithFitness(3.0));

        // Assert - should have low standard deviation with values [3.0, 3.1, 3.0]
        Assert.True(result);
    }
}
