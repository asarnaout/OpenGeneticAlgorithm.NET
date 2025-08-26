using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.Tests.OperatorSelectionPolicies;

/// <summary>
/// Test suite for RoundRobinPolicy covering logic correctness, edge cases, and round-robin behavior.
/// </summary>
public class RoundRobinPolicyTests
{
    /// <summary>
    /// Test operator implementation for testing purposes.
    /// </summary>
    private class TestOperator : BaseOperator
    {
        public string Name { get; }

        public TestOperator(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }

    [Fact]
    public void ApplyOperators_WithValidOperators_SetsOperatorsList()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };

        // Act
        policy.ApplyOperators(operators);

        // Assert - Verify that first selection starts with first operator
        var random = new Random();
        var selected = policy.SelectOperator(random, 0);
        Assert.Equal("Op1", ((TestOperator)selected).Name);
    }

    [Fact]
    public void ApplyOperators_WithEmptyList_ThrowsArgumentException()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var emptyOperators = new List<BaseOperator>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => policy.ApplyOperators(emptyOperators));
        Assert.Contains("At least one operator must be provided", exception.Message);
    }

    [Fact]
    public void ApplyOperators_WithNullList_ThrowsArgumentException()
    {
        // Arrange
        var policy = new RoundRobinPolicy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.ApplyOperators(null!));
    }

    [Fact]
    public void SelectOperator_WithoutApplyingOperators_ThrowsInvalidOperationException()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var random = new Random();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => policy.SelectOperator(random, 0));
        Assert.Contains("No operators available for selection", exception.Message);
    }

    [Fact]
    public void SelectOperator_CyclesThroughOperatorsInOrder()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act & Assert - Test multiple complete cycles
        for (int cycle = 0; cycle < 3; cycle++)
        {
            var selected1 = policy.SelectOperator(random, 0);
            Assert.Equal("Op1", ((TestOperator)selected1).Name);

            var selected2 = policy.SelectOperator(random, 0);
            Assert.Equal("Op2", ((TestOperator)selected2).Name);

            var selected3 = policy.SelectOperator(random, 0);
            Assert.Equal("Op3", ((TestOperator)selected3).Name);
        }
    }

    [Fact]
    public void SelectOperator_WithSingleOperator_AlwaysReturnsSameOperator()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("OnlyOp")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act & Assert - Test multiple selections
        for (int i = 0; i < 10; i++)
        {
            var selected = policy.SelectOperator(random, 0);
            Assert.Equal("OnlyOp", ((TestOperator)selected).Name);
        }
    }

    [Fact]
    public void SelectOperator_WithTwoOperators_AlternatesBetweenThem()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act & Assert - Test alternating pattern
        for (int cycle = 0; cycle < 5; cycle++)
        {
            var selected1 = policy.SelectOperator(random, 0);
            Assert.Equal("Op1", ((TestOperator)selected1).Name);

            var selected2 = policy.SelectOperator(random, 0);
            Assert.Equal("Op2", ((TestOperator)selected2).Name);
        }
    }

    [Fact]
    public void SelectOperator_EqualDistribution_OverManySelections()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3"),
            new TestOperator("Op4")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        var selectionCounts = new Dictionary<string, int>
        {
            ["Op1"] = 0,
            ["Op2"] = 0,
            ["Op3"] = 0,
            ["Op4"] = 0
        };

        // Act - Perform many selections (multiple of operator count for perfect distribution)
        const int totalSelections = 1000; // Should be evenly divisible by 4
        for (int i = 0; i < totalSelections; i++)
        {
            var selected = policy.SelectOperator(random, 0);
            var operatorName = ((TestOperator)selected).Name;
            selectionCounts[operatorName]++;
        }

        // Assert - Each operator should be selected exactly the same number of times
        var expectedCount = totalSelections / operators.Count;
        foreach (var count in selectionCounts.Values)
        {
            Assert.Equal(expectedCount, count);
        }
    }

    [Fact]
    public void SelectOperator_RandomParameterNotUsed_ProducesDeterministicResults()
    {
        // Arrange
        var policy1 = new RoundRobinPolicy();
        var policy2 = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };
        
        policy1.ApplyOperators(operators);
        policy2.ApplyOperators(operators);
        
        var random1 = new Random(42); // Different seeds
        var random2 = new Random(123);

        // Act & Assert - Both policies should produce identical sequences regardless of random seeds
        for (int i = 0; i < 9; i++) // 3 complete cycles
        {
            var selected1 = policy1.SelectOperator(random1, 0);
            var selected2 = policy2.SelectOperator(random2, 0);
            
            Assert.Equal(((TestOperator)selected1).Name, ((TestOperator)selected2).Name);
        }
    }

    [Fact]
    public void ResetAfterApplyOperators_StartsFromFirstOperatorAgain()
    {
        // Arrange
        var policy = new RoundRobinPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act - Select a few operators to advance the index
        policy.SelectOperator(random, 0); // Op1
        policy.SelectOperator(random, 0); // Op2
        
        // Re-apply operators (simulating reconfiguration)
        policy.ApplyOperators(operators);
        
        // Assert - Should start from Op1 again
        var selected = policy.SelectOperator(random, 0);
        Assert.Equal("Op1", ((TestOperator)selected).Name);
    }
}
