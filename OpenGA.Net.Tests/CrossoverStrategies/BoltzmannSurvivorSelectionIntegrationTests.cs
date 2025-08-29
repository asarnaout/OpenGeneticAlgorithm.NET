namespace OpenGA.Net.Tests.CrossoverStrategies;

/// <summary>
/// Integration test demonstrating how to use the new Boltzmann survivor selection strategy
/// with the OpenGARunner.
/// </summary>
public class BoltzmannSurvivorSelectionIntegrationTests
{
    [Fact]
    public void OpenGARunner_WithBoltzmannSurvivorSelectionStrategy_ShouldRunSuccessfully()
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
            .Initialize(initialPopulation)
            .ParentSelection(c => c.Random())
            .Crossover(s => s.RegisterSingle(c => c.OnePointCrossover()))
            .SurvivorSelection(c => c.RegisterSingle(s => s.Boltzmann(temperatureDecayRate: 0.1, initialTemperature: 2.0)))
            .RunToCompletion();

        Assert.NotNull(result);
    }

    [Fact]
    public void OpenGARunner_WithBoltzmannSurvivorSelectionStrategyLinearDecay_ShouldRunSuccessfully()
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
            .Initialize(initialPopulation)
            .ParentSelection(c => c.Random())
            .Crossover(s => s.RegisterSingle(c => c.OnePointCrossover()))
            .SurvivorSelection(c => c.RegisterSingle(s => s.BoltzmannWithLinearDecay(temperatureDecayRate: 0.05, initialTemperature: 1.0)))
            .RunToCompletion();

        Assert.NotNull(result);
    }
}
