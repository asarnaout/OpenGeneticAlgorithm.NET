using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.Tests.OperatorSelectionPolicies;

/// <summary>
/// Test suite for CustomWeightPolicy covering logic correctness, edge cases, and weighted selection behavior.
/// </summary>
public class CustomWeightPolicyTests
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
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1").WithCustomWeight(0.3f),
            new TestOperator("Op2").WithCustomWeight(0.5f),
            new TestOperator("Op3").WithCustomWeight(0.2f)
        };

        // Act
        policy.ApplyOperators(operators);

        // Assert - Should be able to select without throwing
        var random = new Random(42);
        var selected = policy.SelectOperator(random, 0);
        Assert.Contains(selected, operators);
    }

    [Fact]
    public void ApplyOperators_WithEmptyList_ThrowsArgumentException()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var emptyOperators = new List<BaseOperator>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => policy.ApplyOperators(emptyOperators));
        Assert.Equal("At least one operator must be provided. (Parameter 'operators')", exception.Message);
    }

    [Fact]
    public void ApplyOperators_WithNullList_ThrowsArgumentException()
    {
        // Arrange
        var policy = new CustomWeightPolicy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.ApplyOperators(null!));
    }

    [Fact]
    public void SelectOperator_BeforeApplyingOperators_ThrowsInvalidOperationException()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var random = new Random();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => policy.SelectOperator(random, 0));
    }

    [Fact]
    public void SelectOperator_WithSingleOperator_AlwaysReturnsIt()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("OnlyOperator").WithCustomWeight(0.7f)
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act & Assert - Multiple selections should always return the same operator
        for (int i = 0; i < 10; i++)
        {
            var selected = policy.SelectOperator(random, i);
            Assert.Equal("OnlyOperator", ((TestOperator)selected).Name);
        }
    }

    [Fact]
    public void SelectOperator_WithZeroWeights_UsesUniformDistribution()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"), // Default weight is 0
            new TestOperator("Op2"), // Default weight is 0
            new TestOperator("Op3")  // Default weight is 0
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        var selectionCounts = new Dictionary<string, int>
        {
            ["Op1"] = 0,
            ["Op2"] = 0,
            ["Op3"] = 0
        };

        // Act - Perform many selections
        for (int i = 0; i < 3000; i++)
        {
            var selected = policy.SelectOperator(random, i);
            selectionCounts[((TestOperator)selected).Name]++;
        }

        // Assert - Should have roughly equal distribution (within 25% tolerance)
        var expectedCount = 1000;
        var tolerance = 250; // 25% tolerance for statistical variation
        Assert.True(Math.Abs(selectionCounts["Op1"] - expectedCount) <= tolerance, $"Op1 count {selectionCounts["Op1"]} should be within {tolerance} of {expectedCount}");
        Assert.True(Math.Abs(selectionCounts["Op2"] - expectedCount) <= tolerance, $"Op2 count {selectionCounts["Op2"]} should be within {tolerance} of {expectedCount}");
        Assert.True(Math.Abs(selectionCounts["Op3"] - expectedCount) <= tolerance, $"Op3 count {selectionCounts["Op3"]} should be within {tolerance} of {expectedCount}");
    }

    [Fact]
    public void SelectOperator_WithCustomWeights_RespectsWeightDistribution()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1").WithCustomWeight(0.1f), // 10% of total (1.0)
            new TestOperator("Op2").WithCustomWeight(0.6f), // 60% of total (1.0) 
            new TestOperator("Op3").WithCustomWeight(0.3f)  // 30% of total (1.0)
        };
        policy.ApplyOperators(operators);
        var random = new Random(42); // Fixed seed for reproducible test

        var selectionCounts = new Dictionary<string, int>
        {
            ["Op1"] = 0,
            ["Op2"] = 0,
            ["Op3"] = 0
        };

        // Act - Perform many selections
        const int totalSelections = 10000;
        for (int i = 0; i < totalSelections; i++)
        {
            var selected = policy.SelectOperator(random, i);
            selectionCounts[((TestOperator)selected).Name]++;
        }

        // Assert - Check that distribution roughly matches weights (within tolerance)
        var tolerance = 0.05; // 5% tolerance
        Assert.True(Math.Abs((double)selectionCounts["Op1"] / totalSelections - 0.1) <= tolerance, 
            $"Op1 frequency {(double)selectionCounts["Op1"] / totalSelections:F3} should be close to 0.1");
        Assert.True(Math.Abs((double)selectionCounts["Op2"] / totalSelections - 0.6) <= tolerance, 
            $"Op2 frequency {(double)selectionCounts["Op2"] / totalSelections:F3} should be close to 0.6");
        Assert.True(Math.Abs((double)selectionCounts["Op3"] / totalSelections - 0.3) <= tolerance, 
            $"Op3 frequency {(double)selectionCounts["Op3"] / totalSelections:F3} should be close to 0.3");
    }

    [Fact]
    public void SelectOperator_WithNonNormalizedWeights_NormalizesAutomatically()
    {
        // Arrange - Weights that sum to more than 1.0
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1").WithCustomWeight(0.5f), // Will be normalized to 0.25 (0.5/2.0)
            new TestOperator("Op2").WithCustomWeight(1.5f)  // Will be normalized to 0.75 (1.5/2.0)
        };
        policy.ApplyOperators(operators);
        var random = new Random(42);

        var selectionCounts = new Dictionary<string, int>
        {
            ["Op1"] = 0,
            ["Op2"] = 0
        };

        // Act
        const int totalSelections = 8000;
        for (int i = 0; i < totalSelections; i++)
        {
            var selected = policy.SelectOperator(random, i);
            selectionCounts[((TestOperator)selected).Name]++;
        }

        // Assert - Should respect normalized ratio (1:3, so 25%:75%)
        var tolerance = 0.05;
        var op1Frequency = (double)selectionCounts["Op1"] / totalSelections;
        var op2Frequency = (double)selectionCounts["Op2"] / totalSelections;
        
        Assert.True(Math.Abs(op1Frequency - 0.25) <= tolerance, 
            $"Op1 frequency {op1Frequency:F3} should be close to 0.25");
        Assert.True(Math.Abs(op2Frequency - 0.75) <= tolerance, 
            $"Op2 frequency {op2Frequency:F3} should be close to 0.75");
    }

    [Fact]
    public void SelectOperator_WithMixedZeroAndNonZeroWeights_UsesNonZeroWeights()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"), // Default weight is 0
            new TestOperator("Op2").WithCustomWeight(0.8f),
            new TestOperator("Op3") // Default weight is 0
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act - Perform many selections
        var selectedOperators = new HashSet<string>();
        for (int i = 0; i < 1000; i++)
        {
            var selected = policy.SelectOperator(random, i);
            selectedOperators.Add(((TestOperator)selected).Name);
        }

        // Assert - Only Op2 should be selected since it's the only one with non-zero weight
        Assert.Single(selectedOperators);
        Assert.Contains("Op2", selectedOperators);
    }

    [Fact]
    public void SelectOperator_SelectsFromAllAvailableOperators()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1").WithCustomWeight(0.2f),
            new TestOperator("Op2").WithCustomWeight(0.3f),
            new TestOperator("Op3").WithCustomWeight(0.5f)
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act - Perform many selections
        var selectedOperators = new HashSet<string>();
        for (int i = 0; i < 1000; i++)
        {
            var selected = policy.SelectOperator(random, i);
            selectedOperators.Add(((TestOperator)selected).Name);
        }

        // Assert - All operators should be selected at least once over many trials
        Assert.Contains("Op1", selectedOperators);
        Assert.Contains("Op2", selectedOperators);
        Assert.Contains("Op3", selectedOperators);
        Assert.Equal(3, selectedOperators.Count);
    }

    [Fact]
    public void SelectOperator_WithTwoOperators_WorksCorrectly()
    {
        // Arrange
        var policy = new CustomWeightPolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1").WithCustomWeight(0.3f),
            new TestOperator("Op2").WithCustomWeight(0.7f)
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act - Test that both operators can be selected
        var foundOp1 = false;
        var foundOp2 = false;
        
        for (int i = 0; i < 100 && (!foundOp1 || !foundOp2); i++)
        {
            var selected = ((TestOperator)policy.SelectOperator(random, 0)).Name;
            if (selected == "Op1") foundOp1 = true;
            if (selected == "Op2") foundOp2 = true;
        }

        // Assert - Both operators should be selectable
        Assert.True(foundOp1, "Op1 should be selectable");
        Assert.True(foundOp2, "Op2 should be selectable");
    }
}
