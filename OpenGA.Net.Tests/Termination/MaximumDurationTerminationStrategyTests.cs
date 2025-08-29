using OpenGA.Net.Termination;
using OpenGA.Net.ParentSelectorStrategies;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.SurvivorSelectionStrategies;
using System.Diagnostics;

namespace OpenGA.Net.Tests.Termination;

public class MaximumDurationTerminationStrategyTests
{
    private GeneticAlgorithmState CreateMockStateWithDuration(TimeSpan currentDuration)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Thread.Sleep(currentDuration); // Simulate the duration
        stopwatch.Stop();
        return new GeneticAlgorithmState(0, stopwatch, 1.0);
    }

    [Fact]
    public void Constructor_WithZeroDuration_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new MaximumDurationTerminationStrategy<int>(TimeSpan.Zero));
    }

    [Fact]
    public void Constructor_WithNegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new MaximumDurationTerminationStrategy<int>(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Constructor_WithValidDuration_CreatesInstance()
    {
        // Act
        var strategy = new MaximumDurationTerminationStrategy<int>(TimeSpan.FromSeconds(5));

        // Assert
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsLessThanMaximum_ReturnsFalse()
    {
        // Arrange
        var maxDuration = TimeSpan.FromSeconds(1);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMilliseconds(500));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsEqualToMaximum_ReturnsTrue()
    {
        // Arrange
        var maxDuration = TimeSpan.FromSeconds(1);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromSeconds(1));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsGreaterThanMaximum_ReturnsTrue()
    {
        // Arrange
        var maxDuration = TimeSpan.FromSeconds(1);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMilliseconds(1500));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithZeroCurrentDuration_ReturnsFalse()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMilliseconds(500);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.Zero);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WithMillisecondPrecision_WorksCorrectly()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMilliseconds(100);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMilliseconds(101));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithVeryShortDuration_WorksCorrectly()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMilliseconds(10);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMilliseconds(11));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }
}
