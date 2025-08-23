using OpenGA.Net.CrossoverStrategies;
using System.Diagnostics;

namespace OpenGA.Net.Tests;

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

        Assert.Equal(5, offspring.Genes.Count);
        
        var validGeneChoicesForFirstSplice = new[] { 1, 2, 3, 4, 5, 6 };

        for (int i = 0; i < 3; i++)
        {
            Assert.Contains(offspring.Genes[i], validGeneChoicesForFirstSplice);
        }

        for (int i = 3; i < 5; i++)
        {
            Assert.Equal(parentB.Genes[i], offspring.Genes[i]);
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

        Assert.Equal(5, offspring.Genes.Count);
        
        var validGeneChoicesForFirstSplice = new[] { 1, 2, 3, 4, 5, 6 };

        for (int i = 0; i < 3; i++)
        {
            Assert.Contains(offspring.Genes[i], validGeneChoicesForFirstSplice);
        }

        for (int i = 3; i < 5; i++)
        {
            Assert.Equal(parentA.Genes[i], offspring.Genes[i]);
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
    public void CrossoverShouldHandleEmptyChromosomes()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([1, 2, 3]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        Assert.Equal(3, offspring.Genes.Count);
        Assert.Equal(parentB.Genes, offspring.Genes);
    }

    [Fact]
    public void CrossoverShouldHandleBothEmptyChromosomes()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([]);

        var couple = Couple<int>.Pair(parentA, parentB);
        var crossoverStrategy = new UniformCrossoverStrategy<int>();

        var offspring = crossoverStrategy.Crossover(couple, _random).First();

        Assert.Empty(offspring.Genes);
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

        Assert.Equal(100, offspring.Genes.Count);
        
        // First gene should be from either parent
        Assert.Contains(offspring.Genes[0], new[] { 1 }.Concat(parentB.Genes.Take(1)));
        
        // Remaining genes should be from parentB
        for (int i = 1; i < offspring.Genes.Count; i++)
        {
            Assert.Equal(parentB.Genes[i], offspring.Genes[i]);
        }
    }
}
