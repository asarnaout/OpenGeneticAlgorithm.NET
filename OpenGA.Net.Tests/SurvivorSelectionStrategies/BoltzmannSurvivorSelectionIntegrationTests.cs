namespace OpenGA.Net.Tests.SurvivorSelectionStrategies;

/// <summary>
/// Integration tests demonstrating how to use Boltzmann survivor selection strategies
/// with the OpenGARunner.
/// </summary>
public class BoltzmannSurvivorSelectionIntegrationTests
{
    private readonly DummyChromosome[] _initialPopulation = [
        new DummyChromosome([1, 2, 3]),
        new DummyChromosome([4, 5, 6]),
        new DummyChromosome([7, 8, 9]),
        new DummyChromosome([10, 11, 12])
    ];

    [Theory]
    [InlineData("Exponential", 0.1, 2.0)]
    [InlineData("Linear", 0.05, 1.0)]
    public void OpenGARunner_WithBoltzmannSurvivorSelectionStrategies_ShouldRunSuccessfully(
        string decayType, double temperatureDecayRate, double initialTemperature)
    {
        // Act & Assert - Should not throw exceptions
        var runner = OpenGARunner<int>
            .Initialize(_initialPopulation)
            .ParentSelection(c => c.RegisterSingle(s => s.Random()))
            .Crossover(s => s.RegisterSingle(c => c.OnePointCrossover()));

        var result = decayType switch
        {
            "Exponential" => runner.SurvivorSelection(c => c.RegisterSingle(s => 
                s.Boltzmann(temperatureDecayRate: temperatureDecayRate, initialTemperature: initialTemperature)))
                .RunToCompletion(),
            "Linear" => runner.SurvivorSelection(c => c.RegisterSingle(s => 
                s.BoltzmannWithLinearDecay(temperatureDecayRate: temperatureDecayRate, initialTemperature: initialTemperature)))
                .RunToCompletion(),
            _ => throw new ArgumentException($"Unknown decay type: {decayType}")
        };

        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }
}
