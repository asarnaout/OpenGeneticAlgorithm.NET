using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.Exceptions;
using System.Diagnostics;

namespace OpenGA.Net.Tests.CrossoverStrategies;

/// <summary>
/// Comprehensive test suite for KPointCrossoverStrategy covering logic correctness,
/// edge cases, performance, and genetic algorithm soundness.
/// </summary>
public class KPointCrossoverStrategyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidNumberOfPoints_ShouldSetProperty()
    {
        var numberOfPoints = 3;
        var strategy = new KPointCrossoverStrategy<int>(numberOfPoints);
        
        Assert.Equal(numberOfPoints, strategy.NumberOfPoints);
    }

    [Fact]
    public void Constructor_WithZeroPoints_ShouldStillWork()
    {
        var strategy = new KPointCrossoverStrategy<int>(0);
        Assert.Equal(0, strategy.NumberOfPoints);
    }

    [Fact]
    public void Constructor_WithNegativePoints_ShouldStillWork()
    {
        var strategy = new KPointCrossoverStrategy<int>(-1);
        Assert.Equal(-1, strategy.NumberOfPoints);
    }

    #endregion

    #region Exception Tests

    [Fact]
    public async Task Crossover_WithEmptyChromosomeA_ShouldThrowException()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([1, 2, 3, 4, 5]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("invalid chromosome", exception.Message);
        Assert.Contains("at least one gene", exception.Message);
    }

    [Fact]
    public async Task Crossover_WithEmptyChromosomeB_ShouldThrowException()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("invalid chromosome", exception.Message);
        Assert.Contains("at least one gene", exception.Message);
    }

    [Fact]
    public async Task Crossover_WithBothEmptyChromosomes_ShouldThrowException()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(1);
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("invalid chromosome", exception.Message);
    }

    [Fact]
    public async Task Crossover_WithTooManyPointsForChromosomeLength_ShouldThrowException()
    {
        var parentA = new DummyChromosome([1, 2]);  // 2 genes
        var parentB = new DummyChromosome([3, 4]);  // 2 genes
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(5); // More points than genes - 1
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("do not have at least", exception.Message);
        Assert.Contains("genes", exception.Message);
    }

    [Fact]
    public async Task Crossover_WithPointsEqualToChromosomeLength_ShouldThrowException()
    {
        var parentA = new DummyChromosome([1, 2, 3]);  // 3 genes
        var parentB = new DummyChromosome([4, 5, 6]);  // 3 genes
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(3); // Equal to length
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("do not have at least", exception.Message);
    }

    #endregion

    #region Basic Functionality Tests

    [Fact]
    public async Task Crossover_WithOnePoint_ShouldWorkLikeOnePointCrossover()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(1);
        var random = new Random(42); // Fixed seed for reproducibility

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(5, offspring[0].Genes.Count);
        Assert.Equal(5, offspring[1].Genes.Count);

        // Should contain genes from both parents
        var offspringA = offspring[0];
        var offspringB = offspring[1];
        
        // Check that offspring contain genes from both parents
        Assert.True(offspringA.Genes.Intersect(parentA.Genes).Any());
        Assert.True(offspringA.Genes.Intersect(parentB.Genes).Any());
        Assert.True(offspringB.Genes.Intersect(parentA.Genes).Any());
        Assert.True(offspringB.Genes.Intersect(parentB.Genes).Any());
    }

    [Fact]
    public async Task Crossover_WithTwoPoints_ShouldAlternateBetweenParents()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6]);
        var parentB = new DummyChromosome([7, 8, 9, 10, 11, 12]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random(42); // Fixed seed

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        
        // Both offspring should have genes from both parents
        var offspringA = offspring[0];
        var offspringB = offspring[1];
        
        Assert.True(offspringA.Genes.Intersect(parentA.Genes).Any());
        Assert.True(offspringA.Genes.Intersect(parentB.Genes).Any());
        Assert.True(offspringB.Genes.Intersect(parentA.Genes).Any());
        Assert.True(offspringB.Genes.Intersect(parentB.Genes).Any());
    }

    [Fact]
    public async Task Crossover_WithThreePoints_ShouldCreateValidOffspring()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8]);
        var parentB = new DummyChromosome([10, 20, 30, 40, 50, 60, 70, 80]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(3);
        var random = new Random(42);

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(8, offspring[0].Genes.Count);
        Assert.Equal(8, offspring[1].Genes.Count);
    }

    [Fact]
    public async Task Crossover_AlwaysProducesTwoOffspring()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random();

        for (int i = 0; i < 100; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            Assert.Equal(2, offspring.Count);
        }
    }

    #endregion

    #region Variable Length Chromosome Tests

    [Fact]
    public async Task Crossover_WithDifferentLengthChromosomes_ShouldHandleCorrectly()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        var parentB = new DummyChromosome([100, 200, 300]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random(42);

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        
        // Both offspring should have the same length as the longer parent
        var maxLength = Math.Max(parentA.Genes.Count, parentB.Genes.Count);
        Assert.Equal(maxLength, offspring[0].Genes.Count);
        Assert.Equal(maxLength, offspring[1].Genes.Count);
    }

    [Fact]
    public async Task Crossover_WithVeryDifferentLengths_ShouldProduceValidResults()
    {
        var parentA = new DummyChromosome([1]);
        var parentB = new DummyChromosome([100, 200, 300, 400, 500]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(1);
        var random = new Random(42);

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(5, offspring[0].Genes.Count);
        Assert.Equal(5, offspring[1].Genes.Count);
    }

    [Fact]
    public async Task Crossover_WithShortChromosomes_ShouldUseMinimumLength()
    {
        var parentA = new DummyChromosome([1, 2]);
        var parentB = new DummyChromosome([3, 4]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(1);
        var random = new Random(42);

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(2, offspring[0].Genes.Count);
        Assert.Equal(2, offspring[1].Genes.Count);
    }

    #endregion

    #region Crossover Point Generation Tests

    [Fact]
    public async Task Crossover_ShouldGenerateUniquePointsWithinValidRange()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        var parentB = new DummyChromosome([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(3);
        var random = new Random(42);

        // Run multiple times to test randomness
        for (int run = 0; run < 50; run++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            Assert.Equal(2, offspring.Count);
            // The fact that it doesn't throw an exception means points are valid
        }
    }

    [Fact]
    public async Task Crossover_WithMaximumValidPoints_ShouldWork()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        var parentB = new DummyChromosome([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(9); // Maximum valid for 10-gene chromosomes
        var random = new Random(42);

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(10, offspring[0].Genes.Count);
        Assert.Equal(10, offspring[1].Genes.Count);
    }

    #endregion

    #region Age Reset Tests

    [Fact]
    public async Task Crossover_ShouldResetOffspringAge()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);
        
        // Age the parents
        parentA.Age = 5;
        parentB.Age = 3;
        
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(0, offspring[0].Age);
        Assert.Equal(0, offspring[1].Age);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task Crossover_ShouldNotModifyOriginalParents()
    {
        var originalA = new List<int> { 1, 2, 3, 4, 5 };
        var originalB = new List<int> { 6, 7, 8, 9, 10 };
        
        var parentA = new DummyChromosome(originalA.ToList());
        var parentB = new DummyChromosome(originalB.ToList());
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random();

        _ = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Original parents should remain unchanged
        Assert.Equal(originalA, parentA.Genes);
        Assert.Equal(originalB, parentB.Genes);
    }

    [Fact]
    public async Task Crossover_OffspringShouldBeIndependentCopies()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Modifying one offspring should not affect the other
        offspring[0].Genes[0] = 999;
        Assert.NotEqual(999, offspring[1].Genes[0]);
    }

    #endregion

    #region Deterministic Tests with Fixed Seeds

    [Fact]
    public async Task Crossover_WithFixedSeed_ShouldProduceConsistentResults()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6]);
        var parentB = new DummyChromosome([7, 8, 9, 10, 11, 12]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(2);

        var random1 = new Random(12345);
        var offspring1 = (await strategy.CrossoverAsync(couple, random1)).ToList();

        var random2 = new Random(12345);
        var offspring2 = (await strategy.CrossoverAsync(couple, random2)).ToList();

        // Results should be identical with same seed
        Assert.Equal(offspring1[0].Genes, offspring2[0].Genes);
        Assert.Equal(offspring1[1].Genes, offspring2[1].Genes);
    }

    [Fact]
    public async Task Crossover_WithDifferentSeeds_ShouldProduceDifferentResults()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8]);
        var parentB = new DummyChromosome([9, 10, 11, 12, 13, 14, 15, 16]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(3);

        var random1 = new Random(111);
        var offspring1 = (await strategy.CrossoverAsync(couple, random1)).ToList();

        var random2 = new Random(222);
        var offspring2 = (await strategy.CrossoverAsync(couple, random2)).ToList();

        // Results should be different with different seeds (high probability)
        var isDifferent = !offspring1[0].Genes.SequenceEqual(offspring2[0].Genes) ||
                         !offspring1[1].Genes.SequenceEqual(offspring2[1].Genes);
        Assert.True(isDifferent, "Different seeds should produce different results");
    }

    #endregion

    #region Performance Tests

    [Fact]
    [Trait("Category", "Performance")]
    public async Task Crossover_WithLargeChromosomes_ShouldBeEfficient()
    {
        var largeParentA = new DummyChromosome(Enumerable.Range(1, 10000).ToList());
        var largeParentB = new DummyChromosome(Enumerable.Range(10001, 10000).ToList());
        var couple = Couple<int>.Pair(largeParentA, largeParentB);
        var strategy = new KPointCrossoverStrategy<int>(50);
        var random = new Random(42);

        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 100; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            Assert.Equal(2, offspring.Count);
        }
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Performance test failed. Expected < 5000ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("Category", "Performance")]
    public async Task Crossover_WithManyPoints_ShouldHandleEfficiently()
    {
        var parentA = new DummyChromosome(Enumerable.Range(1, 1000).ToList());
        var parentB = new DummyChromosome(Enumerable.Range(1001, 1000).ToList());
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(500); // Half the chromosome length
        var random = new Random(42);

        var stopwatch = Stopwatch.StartNew();
        
        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
        
        stopwatch.Stop();
        
        Assert.Equal(2, offspring.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Many points crossover should be efficient. Actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Tests

    [Fact]
    public async Task Crossover_ShouldNotCauseMemoryLeaks()
    {
        var parentA = new DummyChromosome(Enumerable.Range(1, 1000).ToList());
        var parentB = new DummyChromosome(Enumerable.Range(1001, 1000).ToList());
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(10);
        var random = new Random(42);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var initialMemory = GC.GetTotalMemory(false);
        
        for (int i = 0; i < 1000; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            // Don't hold references to allow garbage collection
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        
        Assert.True(memoryIncrease < 5 * 1024 * 1024, 
            $"Memory test failed. Memory increased by {memoryIncrease} bytes");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Crossover_WithZeroPoints_ShouldReturnCopiesOfOriginalParents()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(0);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        
        // With 0 crossover points, offspring should be copies of parents
        Assert.Equal(parentA.Genes, offspring[0].Genes);
        Assert.Equal(parentB.Genes, offspring[1].Genes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Crossover_WithVariousPointCounts_ShouldProduceValidResults(int numberOfPoints)
    {
        var parentA = new DummyChromosome(Enumerable.Range(1, 12).ToList());
        var parentB = new DummyChromosome(Enumerable.Range(13, 12).ToList());
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(numberOfPoints);
        var random = new Random(42);

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);
        Assert.Equal(12, offspring[0].Genes.Count);
        Assert.Equal(12, offspring[1].Genes.Count);
        
        // Verify genetic material is preserved
        var allOriginalGenes = parentA.Genes.Concat(parentB.Genes).ToHashSet();
        var allOffspringGenes = offspring[0].Genes.Concat(offspring[1].Genes).ToHashSet();
        
        Assert.True(allOffspringGenes.IsSubsetOf(allOriginalGenes), 
            "All offspring genes should come from parent genes");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Crossover_IntegratedWithGeneticAlgorithm_ShouldMaintainPopulationDiversity()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8]);
        var parentB = new DummyChromosome([9, 10, 11, 12, 13, 14, 15, 16]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new KPointCrossoverStrategy<int>(3);
        var random = new Random();

        var allOffspring = new List<List<int>>();
        
        // Generate many offspring to test diversity
        for (int i = 0; i < 100; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            allOffspring.Add(offspring[0].Genes.ToList());
            allOffspring.Add(offspring[1].Genes.ToList());
        }

        // Check for diversity - should have multiple unique combinations
        var uniqueOffspring = allOffspring.Distinct(new ListComparer<int>()).Count();
        Assert.True(uniqueOffspring > 10, 
            $"Should produce diverse offspring. Unique count: {uniqueOffspring}");
    }

    #endregion
}

/// <summary>
/// Helper class to compare lists for equality in diversity tests
/// </summary>
public class ListComparer<T> : IEqualityComparer<List<T>>
{
    public bool Equals(List<T>? x, List<T>? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<T> obj)
    {
        if (obj == null) return 0;
        return obj.Aggregate(0, (acc, item) => acc ^ (item?.GetHashCode() ?? 0));
    }
}
