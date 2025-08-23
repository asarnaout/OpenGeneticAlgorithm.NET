namespace OpenGA.Net.Tests.CrossoverStrategies;

/// <summary>
/// Integration test demonstrating how to use the new Boltzmann replacement strategy
/// with the OpenGARunner.
/// </summary>
public class BoltzmannReplacementIntegrationTests
{
    [Fact]
    public void OpenGARunner_WithBoltzmannReplacementStrategy_ShouldRunSuccessfully()
    {
        // Arrange
        var initialPopulation = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12])
        };

        // Act & Assert - Should not throw exceptions
        var result = OpenGARunner<int>
            .Init(initialPopulation)
            .Epochs(5)
            .ApplyReproductionSelector(c => c.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())
            .ApplyReplacementStrategy(c => c.ApplyBoltzmannReplacementStrategy(temperatureDecayRate: 0.1, initialTemperature: 2.0))
            .RunToCompletion();

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenGARunner_WithBoltzmannReplacementStrategyLinearDecay_ShouldRunSuccessfully()
    {
        // Arrange
        var initialPopulation = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12])
        };

        // Act & Assert - Should not throw exceptions
        var result = OpenGARunner<int>
            .Init(initialPopulation)
            .Epochs(5)
            .ApplyReproductionSelector(c => c.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())
            .ApplyReplacementStrategy(c => c.ApplyBoltzmannReplacementStrategyWithLinearDecay(temperatureDecayRate: 0.05, initialTemperature: 1.0))
            .RunToCompletion();

        Assert.NotNull(result);
    }
}
