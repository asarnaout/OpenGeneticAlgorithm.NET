using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.Exceptions;
using System.Diagnostics;

namespace OpenGA.Net.Tests.CrossoverStrategies;

/// <summary>
/// Comprehensive test suite for OnePointCrossoverStrategy covering logic correctness,
/// edge cases, performance, and genetic algorithm soundness.
/// </summary>
public class OnePointCrossoverStrategyTests
{
    [Fact]
    public async Task CrossoverThrowsOnInvalidChromosome()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10] );
        var couple = Couple<int>.Pair(parentA, parentB);

        var strategy = new TestCrossoverStrategy(1);
        var random = new Random();

        await Assert.ThrowsAsync<InvalidChromosomeException>(async () => (await strategy.CrossoverAsync(couple, random)).ToList());
    }

    [Fact]
    public async Task CrossoverEqualLengthGenesSwapsGenesCorrectly()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5] );
        var parentB = new DummyChromosome([6, 7, 8, 9, 10] );

        var couple = Couple<int>.Pair(parentA, parentB);
        
        var strategy = new TestCrossoverStrategy(2);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);

        var offspringA = offspring[0];
        var offspringB = offspring[1];

        Assert.Equal([ 1, 2, 8, 9, 10 ], offspringA.Genes);
        Assert.Equal([6, 7, 3, 4, 5 ], offspringB.Genes);
    }

    [Fact]
    public async Task CrossoverVariableLengthGenesSwapsGenesCorrectlyCaseA()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5] );
        var parentB = new DummyChromosome([6, 7] );

        var couple = Couple<int>.Pair(parentA, parentB);
        
        var strategy = new TestCrossoverStrategy(2);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);

        var offspringA = offspring[0];
        var offspringB = offspring[1];

        Assert.Equal([ 1, 2 ], offspringA.Genes);
        Assert.Equal([ 6, 7, 3, 4, 5 ], offspringB.Genes);
    }

    [Fact]
    public async Task CrossoverVariableLengthGenesSwapsGenesCorrectlyCaseB()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5] );
        var parentB = new DummyChromosome([6, 7] );

        var couple = Couple<int>.Pair(parentA, parentB);
        
        var strategy = new TestCrossoverStrategy(1);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        Assert.Equal(2, offspring.Count);

        var offspringA = offspring[0];
        var offspringB = offspring[1];

        Assert.Equal([ 1, 7 ], offspringA.Genes);
        Assert.Equal([ 6, 2, 3, 4, 5 ], offspringB.Genes);
    }


    [Fact]
    public void CrossoverPoint_ShouldBeBetweenOneAndMinLength()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new OnePointCrossoverStrategy<int>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Test multiple runs to ensure crossover point is always valid
        for (int i = 0; i < 100; i++)
        {
            var crossoverPoint = strategy.GetCrossoverPoint(couple, random);
            
            // Crossover point should be between 1 and the minimum length (2)
            Assert.InRange(crossoverPoint, 1, 2);
        }
    }

    [Fact]
    public async Task Crossover_ShouldPreserveGeneticMaterialCorrectly()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([10, 20, 30, 40, 50]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new TestCrossoverStrategy(3); // Crossover at position 3
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Offspring A: [1, 2, 3] from A + [40, 50] from B = [1, 2, 3, 40, 50]
        Assert.Equal([1, 2, 3, 40, 50], offspring[0].Genes);
        
        // Offspring B: [10, 20, 30] from B + [4, 5] from A = [10, 20, 30, 4, 5]
        Assert.Equal([10, 20, 30, 4, 5], offspring[1].Genes);
    }

    [Fact]
    public async Task Crossover_WithMinimumLengthChromosomes_ShouldWork()
    {
        var parentA = new DummyChromosome([1, 2]);
        var parentB = new DummyChromosome([10, 20]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new TestCrossoverStrategy(1); // Crossover at position 1
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Offspring A: [1] from A + [20] from B = [1, 20]
        Assert.Equal([1, 20], offspring[0].Genes);
        
        // Offspring B: [10] from B + [2] from A = [10, 2]
        Assert.Equal([10, 2], offspring[1].Genes);
    }

    [Fact]
    public async Task Crossover_WithSingleGeneChromosomes_ShouldThrowException()
    {
        var parentA = new DummyChromosome([42]);
        var parentB = new DummyChromosome([84]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new OnePointCrossoverStrategy<int>();
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("at least 2 genes", exception.Message);
    }

    [Fact]
    public async Task Crossover_WithEmptyChromosomes_ShouldThrowException()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([1, 2, 3]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new OnePointCrossoverStrategy<int>();
        var random = new Random();

        var exception = await Assert.ThrowsAsync<InvalidChromosomeException>(
            async () => (await strategy.CrossoverAsync(couple, random)).ToList());
        
        Assert.Contains("at least 2 genes", exception.Message);
    }

    [Fact]
    public async Task Crossover_ShouldNotModifyOriginalParents()
    {
        var originalA = new int[] { 1, 2, 3, 4, 5 };
        var originalB = new int[] { 10, 20, 30 };
        
        var parentA = new DummyChromosome(originalA.ToList());
        var parentB = new DummyChromosome(originalB.ToList());
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new TestCrossoverStrategy(2);
        var random = new Random();

        _ = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Original parents should remain unchanged
        Assert.Equal(originalA, parentA.Genes);
        Assert.Equal(originalB, parentB.Genes);
    }

    [Fact]
    public async Task Crossover_WithVeryDifferentLengths_ShouldHandleCorrectly()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        var parentB = new DummyChromosome([100, 200]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new TestCrossoverStrategy(2); // Crossover at position 2
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Offspring A: [1, 2] from A + [] from B (nothing after position 2) = [1, 2]
        Assert.Equal([1, 2], offspring[0].Genes);
        
        // Offspring B: [100, 200] from B + [3, 4, 5, 6, 7, 8, 9, 10] from A
        Assert.Equal([100, 200, 3, 4, 5, 6, 7, 8, 9, 10], offspring[1].Genes);
    }

    [Fact]
    public async Task Crossover_AlwaysProducesTwoOffspring()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4]);
        var parentB = new DummyChromosome([5, 6, 7, 8]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new OnePointCrossoverStrategy<int>();
        var random = new Random();

        for (int i = 0; i < 100; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            Assert.Equal(2, offspring.Count);
        }
    }

    [Fact]
    public async Task Crossover_OffspringShouldHaveCorrectTotalGeneCount()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([10, 20, 30]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new TestCrossoverStrategy(2);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Total genes should be preserved (though distributed differently)
        var originalTotalGenes = parentA.Genes.Count + parentB.Genes.Count;
        var offspringTotalGenes = offspring.Sum(o => o.Genes.Count);
        
        Assert.Equal(originalTotalGenes, offspringTotalGenes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Crossover_WithDifferentCrossoverPoints_ShouldProduceValidResults(int crossoverPoint)
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([10, 20, 30, 40, 50]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new TestCrossoverStrategy(crossoverPoint);
        var random = new Random();

        var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();

        // Verify that each offspring has genes from both parents
        var offspringA = offspring[0];
        var offspringB = offspring[1];
        
        // Check that the first part comes from the correct parent
        for (int i = 0; i < crossoverPoint && i < offspringA.Genes.Count; i++)
        {
            Assert.Equal(parentA.Genes[i], offspringA.Genes[i]);
        }
        
        for (int i = 0; i < crossoverPoint && i < offspringB.Genes.Count; i++)
        {
            Assert.Equal(parentB.Genes[i], offspringB.Genes[i]);
        }
    }

    [Fact]
    [Trait("Category", "Performance")]
    public async Task CrossoverPerformance_LargeChromosomes_ShouldBeEfficient()
    {
        // Create large chromosomes for performance testing
        var largeParentA = new DummyChromosome(Enumerable.Range(1, 10000).ToList());
        var largeParentB = new DummyChromosome(Enumerable.Range(10001, 10000).ToList());
        var couple = Couple<int>.Pair(largeParentA, largeParentB);
        var strategy = new OnePointCrossoverStrategy<int>();
        var random = new Random(42);

        var stopwatch = Stopwatch.StartNew();
        
        // Perform many crossover operations
        for (int i = 0; i < 1000; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            Assert.Equal(2, offspring.Count);
        }
        
        stopwatch.Stop();
        
        // Performance should be reasonable (under 1 second for 1000 operations on 10k genes)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Performance test failed. Expected < 1000ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CrossoverMemoryEfficiency_ShouldNotCauseExcessiveAllocations()
    {
        var parentA = new DummyChromosome(Enumerable.Range(1, 1000).ToList());
        var parentB = new DummyChromosome(Enumerable.Range(1001, 1000).ToList());
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new OnePointCrossoverStrategy<int>();
        var random = new Random(42);

        // Force garbage collection before test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var initialMemory = GC.GetTotalMemory(false);
        
        // Perform crossover operations
        for (int i = 0; i < 100; i++)
        {
            var offspring = (await strategy.CrossoverAsync(couple, random)).ToList();
            // Don't hold references to offspring to allow garbage collection
        }
        
        // Force garbage collection after test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        
        // Memory increase should be minimal (less than 1MB for this test)
        Assert.True(memoryIncrease < 1024 * 1024, 
            $"Memory efficiency test failed. Memory increased by {memoryIncrease} bytes");
    }

    [Fact]
    public async Task CrossoverAccuracy_ResultsShouldBeConsistentWithFixedSeed()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([10, 20, 30, 40, 50]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var strategy = new OnePointCrossoverStrategy<int>();

        // Fixed seed should produce consistent results
        var random1 = new Random(12345);
        var random2 = new Random(12345);

        var offspring1 = (await strategy.CrossoverAsync(couple, random1)).ToList();
        var offspring2 = (await strategy.CrossoverAsync(couple, random2)).ToList();

        // Results should be identical with the same seed
        Assert.Equal(offspring1[0].Genes, offspring2[0].Genes);
        Assert.Equal(offspring1[1].Genes, offspring2[1].Genes);
    }

    private class TestCrossoverStrategy(int fixedCrossoverPoint) : OnePointCrossoverStrategy<int>
    {
        private readonly int _fixedCrossoverPoint = fixedCrossoverPoint;

        protected internal override int GetCrossoverPoint(Couple<int> couple, Random random)
        {
            return _fixedCrossoverPoint;
        }
    }
}
