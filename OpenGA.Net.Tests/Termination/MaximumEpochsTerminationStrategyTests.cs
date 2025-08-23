using OpenGA.Net.Termination;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests.Termination;

public class MaximumEpochsTerminationStrategyTests
{
    private OpenGARunner<int> CreateMockRunner(int currentEpoch, int maxEpochs)
    {
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };

        var runner = OpenGARunner<int>.Init(population)
            .Epochs(maxEpochs)
            .ApplyReproductionSelectors(config => config.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategy(config => config.ApplyOnePointCrossoverStrategy())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy());

        // Set the current epoch using reflection since it's internal
        var currentEpochField = typeof(OpenGARunner<int>).GetField("CurrentEpoch", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        currentEpochField?.SetValue(runner, currentEpoch);

        return runner;
    }

    [Fact]
    public void Terminate_WhenCurrentEpochIsLessThanMaxEpochs_ReturnsFalse()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>();
        var runner = CreateMockRunner(currentEpoch: 5, maxEpochs: 10);

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WhenCurrentEpochIsEqualToMaxEpochs_ReturnsTrue()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>();
        var runner = CreateMockRunner(currentEpoch: 10, maxEpochs: 10);

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WhenCurrentEpochIsGreaterThanMaxEpochs_ReturnsTrue()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>();
        var runner = CreateMockRunner(currentEpoch: 15, maxEpochs: 10);

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithZeroCurrentEpoch_ReturnsFalse()
    {
        // Arrange
        var strategy = new MaximumEpochsTerminationStrategy<int>();
        var runner = CreateMockRunner(currentEpoch: 0, maxEpochs: 5);

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.False(result);
    }
}
