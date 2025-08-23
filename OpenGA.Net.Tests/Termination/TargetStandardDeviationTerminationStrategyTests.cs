using OpenGA.Net.Termination;
using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests.Termination;

public class TargetStandardDeviationTerminationStrategyTests
{
    private OpenGARunner<int> CreateMockRunnerWithFitness(double highestFitness)
    {
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };

        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelectors(config => config.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategy(config => config.ApplyOnePointCrossoverStrategy())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy());

        // Mock the highest fitness by creating a chromosome with specific genes
        // Since DummyChromosome calculates fitness as average of genes
        var targetAverage = (int)highestFitness;
        var mockChromosome = new DummyChromosome([targetAverage, targetAverage, targetAverage]);
        
        // Replace the population with our mock chromosome
        var populationField = typeof(OpenGARunner<int>).GetField("_population", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        populationField?.SetValue(runner, new[] { mockChromosome });

        return runner;
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
        var runner = CreateMockRunnerWithFitness(5.0);

        // Act
        var result = strategy.Terminate(runner);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WithTwoIdenticalValues_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1);
        var runner = CreateMockRunnerWithFitness(5.0);

        // Act - call twice with same fitness
        strategy.Terminate(runner);
        var result = strategy.Terminate(runner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithHighVariation_ReturnsFalse()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1);

        // Act - call with varying fitness values
        strategy.Terminate(CreateMockRunnerWithFitness(1.0));
        strategy.Terminate(CreateMockRunnerWithFitness(5.0));
        strategy.Terminate(CreateMockRunnerWithFitness(10.0));
        var result = strategy.Terminate(CreateMockRunnerWithFitness(15.0));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Terminate_WithLowVariationBelowThreshold_ReturnsTrue()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(1.0);

        // Act - call with similar fitness values
        strategy.Terminate(CreateMockRunnerWithFitness(5.0));
        strategy.Terminate(CreateMockRunnerWithFitness(5.1));
        strategy.Terminate(CreateMockRunnerWithFitness(5.2));
        var result = strategy.Terminate(CreateMockRunnerWithFitness(5.1));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Terminate_WithCustomWindow_TracksCorrectNumberOfValues()
    {
        // Arrange
        var strategy = new TargetStandardDeviationTerminationStrategy<int>(0.1, window: 3);

        // Act - call more times than window size
        strategy.Terminate(CreateMockRunnerWithFitness(1.0));
        strategy.Terminate(CreateMockRunnerWithFitness(2.0));
        strategy.Terminate(CreateMockRunnerWithFitness(3.0));
        strategy.Terminate(CreateMockRunnerWithFitness(3.1)); // Should drop the first value (1.0)
        var result = strategy.Terminate(CreateMockRunnerWithFitness(3.0));

        // Assert - should have low standard deviation with values [3.0, 3.1, 3.0]
        Assert.True(result);
    }
}
