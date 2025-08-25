using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.Tests.OperatorSelectionPolicies;

/// <summary>
/// Test suite for OperatorSelectionPolicyConfiguration covering policy configuration methods.
/// </summary>
public class OperatorSelectionPolicyConfigurationTests
{
    [Fact]
    public void ApplyFirstChoicePolicy_ReturnsFirstChoicePolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.ApplyFirstChoicePolicy();

        // Assert
        Assert.IsType<FirstChoicePolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void ApplyRoundRobinPolicy_ReturnsRoundRobinPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.ApplyRoundRobinPolicy();

        // Assert
        Assert.IsType<RoundRobinPolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void ApplyAdaptivePursuitPolicy_ReturnsAdaptivePursuitPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();

        // Act
        var policy = configuration.ApplyAdaptivePursuitPolicy();

        // Assert
        Assert.IsType<AdaptivePursuitPolicy>(policy);
        Assert.Same(policy, configuration.Policy);
    }

    [Fact]
    public void ApplyRoundRobinPolicy_OverridesPreviousPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();
        
        // Act - Apply first policy, then override with round robin
        var firstPolicy = configuration.ApplyFirstChoicePolicy();
        var roundRobinPolicy = configuration.ApplyRoundRobinPolicy();

        // Assert
        Assert.IsType<RoundRobinPolicy>(configuration.Policy);
        Assert.Same(roundRobinPolicy, configuration.Policy);
        Assert.NotSame(firstPolicy, configuration.Policy);
    }

    [Fact]
    public void ApplyFirstChoicePolicy_OverridesRoundRobinPolicy()
    {
        // Arrange
        var configuration = new OperatorSelectionPolicyConfiguration();
        
        // Act - Apply round robin policy, then override with first choice
        var roundRobinPolicy = configuration.ApplyRoundRobinPolicy();
        var firstChoicePolicy = configuration.ApplyFirstChoicePolicy();

        // Assert
        Assert.IsType<FirstChoicePolicy>(configuration.Policy);
        Assert.Same(firstChoicePolicy, configuration.Policy);
        Assert.NotSame(roundRobinPolicy, configuration.Policy);
    }
}
