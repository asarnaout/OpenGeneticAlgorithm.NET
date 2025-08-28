using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.Tests.OperatorSelectionPolicies;

/// <summary>
/// Test suite for OperatorSelectionPolicyConfiguration covering policy configuration methods.
/// </summary>
public class OperatorSelectionPolicyConfigurationTests
{
    [Fact]
    public void FirstChoice_ReturnsFirstChoicePolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.FirstChoice();

        // Assert
        Assert.IsType<FirstChoicePolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void RoundRobin_ReturnsRoundRobinPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.RoundRobin();

        // Assert
        Assert.IsType<RoundRobinPolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void Random_ReturnsRandomChoicePolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.Random();

        // Assert
        Assert.IsType<RandomChoicePolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void CustomWeights_ReturnsCustomWeightPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.CustomWeights();

        // Assert
        Assert.IsType<CustomWeightPolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void AdaptivePursuit_ReturnsAdaptivePursuitPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.AdaptivePursuit();

        // Assert
        Assert.IsType<AdaptivePursuitPolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void Random_OverridesPreviousPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();
        
        // Act - Apply first policy, then override with random choice
        var firstPolicy = configuration.FirstChoice();
        var randomChoicePolicy = configuration.Random();

        // Assert
        Assert.IsType<RandomChoicePolicy>(configuration.Policy);
        Assert.Same(randomChoicePolicy, configuration.Policy);
        Assert.NotSame(firstPolicy, configuration.Policy);
    }

    [Fact]
    public void RoundRobin_OverridesPreviousPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();
        
        // Act - Apply first policy, then override with round robin
        var firstPolicy = configuration.FirstChoice();
        var roundRobinPolicy = configuration.RoundRobin();

        // Assert
        Assert.IsType<RoundRobinPolicy>(configuration.Policy);
        Assert.Same(roundRobinPolicy, configuration.Policy);
        Assert.NotSame(firstPolicy, configuration.Policy);
    }

    [Fact]
    public void FirstChoice_OverridesRoundRobinPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();
        
        // Act - Apply round robin policy, then override with first choice
        var roundRobinPolicy = configuration.RoundRobin();
        var firstChoicePolicy = configuration.FirstChoice();

        // Assert
        Assert.IsType<FirstChoicePolicy>(configuration.Policy);
        Assert.Same(firstChoicePolicy, configuration.Policy);
        Assert.NotSame(roundRobinPolicy, configuration.Policy);
    }
}
