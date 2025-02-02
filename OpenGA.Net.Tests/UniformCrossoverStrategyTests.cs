using OpenGA.Net.CrossoverStrategies;

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
}
