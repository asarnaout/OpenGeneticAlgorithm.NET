using OpenGA.Net.Termination;
using System.Diagnostics;

namespace OpenGA.Net.Tests.Termination;

public class TargetFitnessTerminationStrategyTests
{
    private GeneticAlgorithmState CreateMockState(double highestFitness)
    {
        var stopwatch = new Stopwatch();
        return new GeneticAlgorithmState(1, stopwatch, highestFitness);
    }

    [Fact]
    public void Terminate_WhenHighestFitnessIsLessThanTargetFitness_ReturnsFalse()
    {
        // Arrange
        var strategy = new TargetFitnessTerminationStrategy<int>(10.0);
        var state = CreateMockState(highestFitness: 5.0);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WhenHighestFitnessIsEqualToTargetFitness_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetFitnessTerminationStrategy<int>(10.0);
        var state = CreateMockState(highestFitness: 10.0);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WhenHighestFitnessIsGreaterThanTargetFitness_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetFitnessTerminationStrategy<int>(10.0);
        var state = CreateMockState(highestFitness: 15.0);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithZeroTargetFitness_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetFitnessTerminationStrategy<int>(0.0);
        var state = CreateMockState(highestFitness: 5.0);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithNegativeTargetFitness_ReturnsCorrectResult()
    {
        // Arrange
        var strategy = new TargetFitnessTerminationStrategy<int>(-5.0);
        var stateBelowTarget = CreateMockState(highestFitness: -10.0);
        var stateAboveTarget = CreateMockState(highestFitness: 0.0);

        // Act
        var resultBelowTarget = strategy.Terminate(stateBelowTarget);
        var resultAboveTarget = strategy.Terminate(stateAboveTarget);

        // Assert
        Assert.False(resultBelowTarget);
        Assert.True(resultAboveTarget);
    }

    [Fact]
    public void Terminate_WithVerySmallDifference_ReturnsCorrectResult()
    {
        // Arrange
        var strategy = new TargetFitnessTerminationStrategy<int>(10.0);
        var stateJustBelow = CreateMockState(highestFitness: 9.9999999);
        var stateJustAbove = CreateMockState(highestFitness: 10.0000001);

        // Act
        var resultJustBelow = strategy.Terminate(stateJustBelow);
        var resultJustAbove = strategy.Terminate(stateJustAbove);

        // Assert
        Assert.False(resultJustBelow);
        Assert.True(resultJustAbove);
    }

    [Fact]
    public void TerminationStrategyConfiguration_TargetFitness_AddsStrategyCorrectly()
    {
        // Arrange
        var config = new TerminationStrategyConfiguration<int>();
        var targetFitness = 42.0;

        // Act
        var result = config.TargetFitness(targetFitness);

        // Assert
        Assert.Same(config, result); // Should return same instance for chaining
        Assert.Single(config.TerminationStrategies); // Should have one strategy
        Assert.IsType<TargetFitnessTerminationStrategy<int>>(config.TerminationStrategies[0]);
        
        // Test that the strategy works correctly
        var state = CreateMockState(highestFitness: targetFitness);
        Assert.True(config.ShouldTerminate(state));
        
        var stateBelow = CreateMockState(highestFitness: targetFitness - 1);
        Assert.False(config.ShouldTerminate(stateBelow));
    }
}
