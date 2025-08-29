using OpenGA.Net.OperatorSelectionPolicies;
using OpenGA.Net.SurvivorSelectionStrategies;

namespace OpenGA.Net.Tests.SurvivorSelectionStrategies;

public class MultiSurvivorSelectionStrategyConfigurationTests
{
    [Fact]
    public void RegisterMulti_WithMultipleStrategies_ShouldRegisterAllStrategies()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act
        registration.RegisterMulti(m => m
            .Elitist(0.1f)
            .Tournament(3)
            .Random()
        );

        // Assert
        var strategies = registration.GetRegisteredSurvivorSelectionStrategies();
        Assert.Equal(3, strategies.Count);
        Assert.IsType<ElitistSurvivorSelectionStrategy<int>>(strategies[0]);
        Assert.IsType<TournamentSurvivorSelectionStrategy<int>>(strategies[1]);
        Assert.IsType<RandomEliminationSurvivorSelectionStrategy<int>>(strategies[2]);
    }

    [Fact]
    public void RegisterMulti_WithCustomWeights_ShouldApplyCustomWeightPolicy()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act
        registration.RegisterMulti(m => m
            .Elitist(0.1f, 0.7f)
            .Tournament(3, true, 0.3f)
        );

        registration.ValidateAndDefault(new Random());

        // Assert
        var policy = registration.GetSurvivorSelectionSelectionPolicy();
        Assert.IsType<CustomWeightPolicy>(policy);
        
        var strategies = registration.GetRegisteredSurvivorSelectionStrategies();
        Assert.Equal(0.7f, strategies[0].CustomWeight);
        Assert.Equal(0.3f, strategies[1].CustomWeight);
    }

    [Fact]
    public void RegisterMulti_WithoutCustomWeights_ShouldApplyAdaptivePursuitPolicy()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act
        registration.RegisterMulti(m => m
            .Elitist(0.1f)
            .Tournament(3)
        );

        registration.ValidateAndDefault(new Random());

        // Assert
        var policy = registration.GetSurvivorSelectionSelectionPolicy();
        Assert.IsType<AdaptivePursuitPolicy>(policy);
    }

    [Fact]
    public void RegisterMulti_WithExplicitPolicy_ShouldUseSpecifiedPolicy()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act
        registration.RegisterMulti(m => m
            .Elitist(0.1f)
            .Tournament(3)
            .WithPolicy(p => p.Random())
        );

        registration.ValidateAndDefault(new Random());

        // Assert
        var policy = registration.GetSurvivorSelectionSelectionPolicy();
        Assert.IsType<RandomChoicePolicy>(policy);
    }

    [Fact]
    public void RegisterSingle_ShouldApplyFirstChoicePolicy()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act
        registration.RegisterSingle(s => s.Elitist(0.1f));
        registration.ValidateAndDefault(new Random());

        // Assert
        var policy = registration.GetSurvivorSelectionSelectionPolicy();
        Assert.IsType<FirstChoicePolicy>(policy);
        
        var strategies = registration.GetRegisteredSurvivorSelectionStrategies();
        Assert.Single(strategies);
        Assert.IsType<ElitistSurvivorSelectionStrategy<int>>(strategies[0]);
    }

    [Fact]
    public void OverrideOffspringGenerationRate_ShouldSetCorrectValue()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act
        registration.RegisterSingle(s => s.Elitist())
                   .OverrideOffspringGenerationRate(0.6f);

        // Assert
        Assert.Equal(0.6f, registration.GetOffspringGenerationRateOverride());
    }

    [Fact]
    public void OverrideOffspringGenerationRate_WithInvalidValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var registration = new SurvivorSelectionStrategyRegistration<int>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            registration.OverrideOffspringGenerationRate(-0.1f));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            registration.OverrideOffspringGenerationRate(1.1f));
    }
}
