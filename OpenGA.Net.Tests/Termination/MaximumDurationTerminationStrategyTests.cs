using OpenGA.Net.Termination;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests.Termination;

public class MaximumDurationTerminationStrategyTests
{
    private OpenGARunner<int> CreateMockRunnerWithDuration(TimeSpan currentDuration)
    {
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };

        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategy(config => config.ApplyOnePointCrossoverStrategy())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy());

        // Set the current duration using reflection since it's internal
        var currentDurationField = typeof(OpenGARunner<int>).GetField("CurrentDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        currentDurationField?.SetValue(runner, currentDuration);

        return runner;
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
        var runner = CreateMockRunnerWithDuration(TimeSpan.FromMinutes(5));

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsEqualToMaximum_ReturnsTrue()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMinutes(10);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var runner = CreateMockRunnerWithDuration(TimeSpan.FromMinutes(10));

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WhenCurrentDurationIsGreaterThanMaximum_ReturnsTrue()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMinutes(10);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var runner = CreateMockRunnerWithDuration(TimeSpan.FromMinutes(15));

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithZeroCurrentDuration_ReturnsFalse()
    {
        // Arrange
        var maxDuration = TimeSpan.FromSeconds(30);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var runner = CreateMockRunnerWithDuration(TimeSpan.Zero);

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WithMillisecondPrecision_WorksCorrectly()
    {
        // Arrange
        var maxDuration = TimeSpan.FromMilliseconds(500);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var runner = CreateMockRunnerWithDuration(TimeSpan.FromMilliseconds(501));

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithVeryShortDuration_WorksCorrectly()
    {
        // Arrange
        var maxDuration = TimeSpan.FromTicks(1000);
        var strategy = new MaximumDurationTerminationStrategy<int>(maxDuration);
        var runner = CreateMockRunnerWithDuration(TimeSpan.FromTicks(1001));

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }
}
