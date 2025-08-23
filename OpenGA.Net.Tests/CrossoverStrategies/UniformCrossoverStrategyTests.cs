using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.Exceptions;
using System.Diagnostics;

namespace OpenGA.Net.Tests.CrossoverStrategies;

public class UniformCrossoverStrategyTests
{
    private readonly Random _random = new();

    [Fact]
    public void CrossoverShouldGenerateOffspringWithValidParentGenes()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        Assert.Equal(5, offspring.Genes.Count);

        foreach (var gene in offspring.Genes)
        {
            Assert.Contains(gene, parentA.Genes.Concat(parentB.Genes));
        }
    }

    [Fact]
    public void CrossoverShouldHandleUnequalLengthParentsA()
    {
        var parentA = new DummyChromosome([1, 2, 3]);
        var parentB = new DummyChromosome([4, 5, 6, 7, 8]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        // Offspring can be either minLength (3) or maxLength (5) due to coin flip
        Assert.True(offspring.Genes.Count == 3 || offspring.Genes.Count == 5, 
            $"Expected offspring length to be 3 or 5, but was {offspring.Genes.Count}");
        
        var validGeneChoicesForOverlapRegion = new[] { 1, 2, 3, 4, 5, 6 };

        // Check overlapping region (first 3 genes) - should come from either parent
        for (int i = 0; i < 3; i++)
        {
            Assert.Contains(offspring.Genes[i], validGeneChoicesForOverlapRegion);
        }

        // If offspring has 5 genes, check the non-overlapping region (positions 3-4)
        if (offspring.Genes.Count == 5)
        {
            for (int i = 3; i < 5; i++)
            {
                Assert.Equal(parentB.Genes[i], offspring.Genes[i]);
            }
        }
    }

        [Fact]
    public void CrossoverShouldHandleUnequalLengthParentsB()
    {
        var parentA = new DummyChromosome([4, 5, 6, 7, 8]);
        var parentB = new DummyChromosome([1, 2, 3]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        // Offspring can be either minLength (3) or maxLength (5) due to coin flip
        Assert.True(offspring.Genes.Count == 3 || offspring.Genes.Count == 5, 
            $"Expected offspring length to be 3 or 5, but was {offspring.Genes.Count}");
        
        var validGeneChoicesForOverlapRegion = new[] { 1, 2, 3, 4, 5, 6 };

        // Check overlapping region (first 3 genes) - should come from either parent
        for (int i = 0; i < 3; i++)
        {
            Assert.Contains(offspring.Genes[i], validGeneChoicesForOverlapRegion);
        }

        // If offspring has 5 genes, check the non-overlapping region (positions 3-4)
        if (offspring.Genes.Count == 5)
        {
            for (int i = 3; i < 5; i++)
            {
                Assert.Equal(parentA.Genes[i], offspring.Genes[i]);
            }
        }
    }

    [Fact]
    public void CrossoverShouldNotModifyOriginalParents()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var originalParentA = new List<int>(parentA.Genes);
        var originalParentB = new List<int>(parentB.Genes);

        _ = crossoverStrategy.Crossover(couple, _random).First();

        Assert.Equal(originalParentA, parentA.Genes);
        Assert.Equal(originalParentB, parentB.Genes);
    }

    [Fact]
    public void CrossoverShouldPreserveUniformRandomDistribution()
    {
        var parentA = new DummyChromosome([0, 0, 0, 0, 0]);
        var parentB = new DummyChromosome([1, 1, 1, 1, 1]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        int zeroCount = 0, oneCount = 0;
        var iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            var offspring = crossoverStrategy.Crossover(couple, _random).First();
            zeroCount += offspring.Genes.Count(g => g == 0);
            oneCount += offspring.Genes.Count(g => g == 1);
        }

        double ratio = (double)zeroCount / (zeroCount + oneCount);

        Assert.InRange(ratio, 0.45, 0.55);
    }

    [Fact]
    public void CrossoverShouldThrowWhenParentAIsEmpty()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([1, 2, 3]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var exception = Assert.Throws<InvalidChromosomeException>(() => 
            crossoverStrategy.Crossover(couple, _random).First());
            
        Assert.Equal("Parent A has null or empty genes collection.", exception.Message);
    }

    [Fact]
    public void CrossoverShouldThrowWhenParentBIsEmpty()
    {
        var parentA = new DummyChromosome([1, 2, 3]);
        var parentB = new DummyChromosome([]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var exception = Assert.Throws<InvalidChromosomeException>(() => 
            crossoverStrategy.Crossover(couple, _random).First());
            
        Assert.Equal("Parent B has null or empty genes collection.", exception.Message);
    }

    [Fact]
    public void CrossoverShouldThrowWhenBothParentsAreEmpty()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var exception = Assert.Throws<InvalidChromosomeException>(() => 
            crossoverStrategy.Crossover(couple, _random).First());
            
        Assert.Equal("Parent A has null or empty genes collection.", exception.Message);
    }

    [Fact]
    public void CrossoverShouldHandleSingleGeneChromosomes()
    {
        var parentA = new DummyChromosome([1]);
        var parentB = new DummyChromosome([2]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        Assert.Single(offspring.Genes);
        Assert.Contains(offspring.Genes[0], new[] { 1, 2 });
    }

    [Fact]
    public void CrossoverShouldMaintainUniformDistributionWithUnequalLengths()
    {
        var parentA = new DummyChromosome([0, 0]);
        var parentB = new DummyChromosome([1, 1, 1, 1]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        int zeroCount = 0, oneCount = 0;
        var iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            var offspring = crossoverStrategy.Crossover(couple, _random).First();
            
            // Count genes in overlap region (first 2 positions) - should be roughly 50/50
            zeroCount += offspring.Genes.Take(2).Count(g => g == 0);
            oneCount += offspring.Genes.Take(2).Count(g => g == 1);
            
            // Non-overlap region (last 2 positions) should always be from parentB
            Assert.True(offspring.Genes.Skip(2).All(g => g == 1));
        }

        double ratio = (double)zeroCount / (zeroCount + oneCount);
        Assert.InRange(ratio, 0.45, 0.55);
    }

    [Fact]
    public void CrossoverPerformance_LargeChromosomes_ShouldBeEfficient()
    {
        // Create large chromosomes for performance testing
        var largeGenesA = Enumerable.Range(0, 10000).ToList();
        var largeGenesB = Enumerable.Range(10000, 10000).ToList();
        
        var parentA = new DummyChromosome(largeGenesA);
        var parentB = new DummyChromosome(largeGenesB);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var stopwatch = Stopwatch.StartNew();

        // Perform multiple crossover operations
        for (int i = 0; i < 1000; i++)
        {
            _ = crossoverStrategy.Crossover(couple, _random).First();
        }

        stopwatch.Stop();

        // Performance should be reasonable (under 1 second for 1000 operations on 10k genes)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Performance test failed. Expected < 1000ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void CrossoverShouldHandleVeryUnequalLengths()
    {
        var parentA = new DummyChromosome([1]);
        var parentB = new DummyChromosome(Enumerable.Range(2, 100).ToList());

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        // Offspring can be either minLength (1) or maxLength (100) due to coin flip
        Assert.True(offspring.Genes.Count == 1 || offspring.Genes.Count == 100, 
            $"Expected offspring length to be 1 or 100, but was {offspring.Genes.Count}");
        
        // First gene should be from either parent
        Assert.Contains(offspring.Genes[0], new[] { 1, 2 });
        
        // If offspring has 100 genes, check the remaining genes from parentB
        if (offspring.Genes.Count == 100)
        {
            for (int i = 1; i < offspring.Genes.Count; i++)
            {
                Assert.Equal(parentB.Genes[i], offspring.Genes[i]);
            }
        }
    }
    
    [Fact]
    public void CrossoverShouldProduceVariableLengthOffspringWithCoinFlip()
    {
        var parentA = new DummyChromosome([1, 2, 3]);
        var parentB = new DummyChromosome([4, 5, 6, 7, 8, 9, 10]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var lengthCounts = new Dictionary<int, int>();
        
        // Run crossover many times to verify coin flip behavior
        for (int trial = 0; trial < 1000; trial++)
        {
            var offspring = crossoverStrategy.Crossover(couple, new Random(trial)).First();
            
            // Track length distribution
            lengthCounts[offspring.Genes.Count] = lengthCounts.GetValueOrDefault(offspring.Genes.Count, 0) + 1;
            
            // Offspring should have either minLength (3) or maxLength (7) - no other lengths
            Assert.True(offspring.Genes.Count == 3 || offspring.Genes.Count == 7, 
                $"Expected offspring length to be 3 or 7, but was {offspring.Genes.Count}");
            
            // Check overlapping region (first 3 genes) - should come from either parent
            for (int i = 0; i < 3; i++)
            {
                Assert.Contains(offspring.Genes[i], new[] { parentA.Genes[i], parentB.Genes[i] });
            }
            
            // If offspring has 7 genes, check non-overlapping region comes from parentB
            if (offspring.Genes.Count == 7)
            {
                for (int i = 3; i < 7; i++)
                {
                    Assert.Equal(parentB.Genes[i], offspring.Genes[i]);
                }
            }
        }
        
        // Should have produced offspring of both possible lengths due to coin flip
        Assert.True(lengthCounts.ContainsKey(3), "Should produce some offspring with length 3 (shorter parent)");
        Assert.True(lengthCounts.ContainsKey(7), "Should produce some offspring with length 7 (longer parent)");
        
        // Should be roughly 50-50 distribution due to coin flip (allowing some variance)
        var shortCount = lengthCounts.GetValueOrDefault(3, 0);
        var longCount = lengthCounts.GetValueOrDefault(7, 0);
        var ratio = (double)shortCount / (shortCount + longCount);
        Assert.InRange(ratio, 0.4, 0.6); // Should be around 0.5 with some tolerance
    }
}
