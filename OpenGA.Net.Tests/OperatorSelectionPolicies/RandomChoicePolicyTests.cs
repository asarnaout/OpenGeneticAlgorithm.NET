using OpenGA.Net.OperatorSelectionPolicies;

namespace OpenGA.Net.Tests.OperatorSelectionPolicies;

/// <summary>
/// Test suite for RandomChoicePolicy covering logic correctness, edge cases, and random selection behavior.
/// </summary>
public class RandomChoicePolicyTests
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
        var policy = new RandomChoicePolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
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
        var policy = new RandomChoicePolicy();
        var emptyOperators = new List<BaseOperator>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => policy.ApplyOperators(emptyOperators));
        Assert.Contains("At least one operator must be provided", exception.Message);
    }

    [Fact]
    public void ApplyOperators_WithNullList_ThrowsArgumentException()
    {
        // Arrange
        var policy = new RandomChoicePolicy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.ApplyOperators(null!));
    }

    [Fact]
    public void SelectOperator_WithoutApplyingOperators_ThrowsInvalidOperationException()
    {
        // Arrange
        var policy = new RandomChoicePolicy();
        var random = new Random();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => policy.SelectOperator(random, 0));
        Assert.Contains("No operators available for selection", exception.Message);
    }

    [Fact]
    public void SelectOperator_WithNullRandom_ThrowsArgumentNullException()
    {
        // Arrange
        var policy = new RandomChoicePolicy();
        var operators = new List<BaseOperator> { new TestOperator("Op1") };
        policy.ApplyOperators(operators);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => policy.SelectOperator(null!, 0));
        Assert.Contains("Random number generator cannot be null", exception.Message);
    }

    [Fact]
    public void SelectOperator_WithSingleOperator_AlwaysReturnsSameOperator()
    {
        // Arrange
        var policy = new RandomChoicePolicy();
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
    public void SelectOperator_SelectsFromAllAvailableOperators()
    {
        // Arrange
        var policy = new RandomChoicePolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act - Perform many selections
        var selectedOperators = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            var selected = policy.SelectOperator(random, 0);
            selectedOperators.Add(((TestOperator)selected).Name);
        }

        // Assert - All operators should be selected at least once over many trials
        Assert.Contains("Op1", selectedOperators);
        Assert.Contains("Op2", selectedOperators);
        Assert.Contains("Op3", selectedOperators);
    }

    [Fact]
    public void SelectOperator_ApproximatelyEqualDistribution_OverManySelections()
    {
        // Arrange
        var policy = new RandomChoicePolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3"),
            new TestOperator("Op4")
        };
        policy.ApplyOperators(operators);
        var random = new Random(42); // Fixed seed for reproducibility

        var selectionCounts = new Dictionary<string, int>
        {
            ["Op1"] = 0,
            ["Op2"] = 0,
            ["Op3"] = 0,
            ["Op4"] = 0
        };

        // Act - Perform many selections
        const int totalSelections = 10000;
        for (int i = 0; i < totalSelections; i++)
        {
            var selected = policy.SelectOperator(random, 0);
            var operatorName = ((TestOperator)selected).Name;
            selectionCounts[operatorName]++;
        }

        // Assert - Each operator should be selected approximately equal times
        var expectedCount = totalSelections / operators.Count;
        var tolerance = expectedCount * 0.1; // 10% tolerance for randomness

        foreach (var (operatorName, count) in selectionCounts)
        {
            Assert.True(Math.Abs(count - expectedCount) < tolerance, 
                $"Operator {operatorName} was selected {count} times, expected ~{expectedCount} (±{tolerance})");
        }
    }

    [Fact]
    public void SelectOperator_ProducesRandomResults_WithDifferentSeeds()
    {
        // Arrange
        var policy1 = new RandomChoicePolicy();
        var policy2 = new RandomChoicePolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };
        
        policy1.ApplyOperators(operators);
        policy2.ApplyOperators(operators);
        
        var random1 = new Random(42);
        var random2 = new Random(123);

        // Act - Generate sequences from both policies
        var sequence1 = new List<string>();
        var sequence2 = new List<string>();
        
        for (int i = 0; i < 20; i++)
        {
            sequence1.Add(((TestOperator)policy1.SelectOperator(random1, 0)).Name);
            sequence2.Add(((TestOperator)policy2.SelectOperator(random2, 0)).Name);
        }

        // Assert - Sequences should be different (extremely unlikely to be identical)
        Assert.NotEqual(sequence1, sequence2);
    }

    [Fact]
    public void SelectOperator_ProducesReproducibleResults_WithSameSeed()
    {
        // Arrange
        var policy1 = new RandomChoicePolicy();
        var policy2 = new RandomChoicePolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2"),
            new TestOperator("Op3")
        };
        
        policy1.ApplyOperators(operators);
        policy2.ApplyOperators(operators);
        
        var random1 = new Random(42);
        var random2 = new Random(42); // Same seed

        // Act - Generate sequences from both policies
        var sequence1 = new List<string>();
        var sequence2 = new List<string>();
        
        for (int i = 0; i < 20; i++)
        {
            sequence1.Add(((TestOperator)policy1.SelectOperator(random1, 0)).Name);
            sequence2.Add(((TestOperator)policy2.SelectOperator(random2, 0)).Name);
        }

        // Assert - Sequences should be identical with same seed
        Assert.Equal(sequence1, sequence2);
    }

    [Fact]
    public void SelectOperator_AllOperatorsSelectable_WithTwoOperators()
    {
        // Arrange
        var policy = new RandomChoicePolicy();
        var operators = new List<BaseOperator>
        {
            new TestOperator("Op1"),
            new TestOperator("Op2")
        };
        policy.ApplyOperators(operators);
        var random = new Random();

        // Act - Test that both operators can be selected
        var foundOp1 = false;
        var foundOp2 = false;
        
        for (int i = 0; i < 50 && (!foundOp1 || !foundOp2); i++)
        {
            var selected = ((TestOperator)policy.SelectOperator(random, 0)).Name;
            if (selected == "Op1") foundOp1 = true;
            if (selected == "Op2") foundOp2 = true;
        }

        // Assert
        Assert.True(foundOp1, "Op1 should be selectable");
        Assert.True(foundOp2, "Op2 should be selectable");
    }
}
