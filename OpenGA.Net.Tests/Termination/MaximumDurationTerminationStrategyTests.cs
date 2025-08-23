using OpenGA.Net.Termination;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests.Termination;

public class MaximumDurationTerminationStrategyTests
{
    private GeneticAlgorithmState CreateMockStateWithDuration(TimeSpan currentDuration)
    {
        return new GeneticAlgorithmState(0, 100, currentDuration, 1.0);
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
        var strategy = new MaximumDurationTerminationStrategy<int>(TimeSpan.FromMinutes(5));

        // Assert
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsLessThanMaximum_ReturnsFalse()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMinutes(10);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMinutes(5));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsEqualToMaximum_ReturnsTrue()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMinutes(10);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMinutes(10));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsGreaterThanMaximum_ReturnsTrue()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMinutes(10);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMinutes(15));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithZeroCurrentDuration_ReturnsFalse()
    {
        // Arrange
        var maxDuration = TimeSpan.FromSeconds(30);
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
        var maxDuration = TimeSpan.FromMilliseconds(500);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromMilliseconds(501));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithVeryShortDuration_WorksCorrectly()
    {
        // Arrange
        var maxDuration = TimeSpan.FromTicks(1000);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var state = CreateMockStateWithDuration(TimeSpan.FromTicks(1001));

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }
}
