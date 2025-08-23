using OpenGA.Net.Termination;

namespace OpenGA.Net.Tests.Termination;

public class MaximumEpochsTerminationStrategyTests
{
    private GeneticAlgorithmState CreateMockState(int currentEpoch)
    {
        return new GeneticAlgorithmState(currentEpoch, TimeSpan.Zero, 1.0);
    }

    [Fact]
    public void Terminate_WhenCurrentEpochIsLessThanMaxEpochs_ReturnsFalse()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>(10);
        var state = CreateMockState(currentEpoch: 5);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WhenCurrentEpochIsEqualToMaxEpochs_ReturnsTrue()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>(10);
        var state = CreateMockState(currentEpoch: 10);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WhenCurrentEpochIsGreaterThanMaxEpochs_ReturnsTrue()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>(10);
        var state = CreateMockState(currentEpoch: 15);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithZeroCurrentEpoch_ReturnsFalse()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>(5);
        var state = CreateMockState(currentEpoch: 0);

        // Act
        var result = strategy.Terminate(state);

        // Assert
        Assert.False(result);
    }
}
