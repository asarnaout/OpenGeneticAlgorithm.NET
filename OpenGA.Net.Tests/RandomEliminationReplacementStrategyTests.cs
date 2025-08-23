using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests;

public class RandomEliminationReplacementStrategyTests
{
    [Fact]
    public void Constructor_ShouldSucceed()
    {
        // Arrange & Act
        var strategy = new RandomEliminationReplacementStrategy<int>();

        // Assert - Should not throw any exceptions
        Assert.NotNull(strategy);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyPopulation_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        var random = new Random(42);
        var population = Array.Empty<Chromosome<int>>();
        var offspring = new[] { new DummyChromosome([1, 2, 3]) };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyOffspring_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        var random = new Random(42);
        var population = new[] { new DummyChromosome([1, 2, 3]) };
        var offspring = Array.Empty<Chromosome<int>>();

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_ShouldEliminateExactlyEliminationsNeeded()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15])
        };

        var offspring = new[]
        {
            new DummyChromosome([16, 17, 18]),
            new DummyChromosome([19, 20, 21])
        };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        // Should eliminate exactly as many chromosomes as we have offspring
        Assert.Equal(offspring.Length, result.Count());
        
        // All eliminated chromosomes should be from the original population
        foreach (var eliminated in result)
        {
            Assert.Contains(eliminated, population);
        }
    }

    [Fact]
    public void SelectChromosomesForElimination_WithMoreOffspringThanPopulation_ShouldEliminateEntirePopulation()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };

        var offspring = new[]
        {
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15])
        };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        // Should eliminate entire population since offspring > population
        Assert.Equal(population.Length, result.Count());
        Assert.Equal(population.Length, result.Intersect(population).Count());
    }

    [Fact]
    public void SelectChromosomesForElimination_ShouldSelectRandomly()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15])
        };

        var offspring = new[]
        {
            new DummyChromosome([16, 17, 18]),
            new DummyChromosome([19, 20, 21])
        };

        // Act with different random seeds
        var random1 = new Random(42);
        var random2 = new Random(123);
        
        var result1 = strategy.SelectChromosomesForElimination(population, offspring, random1).ToList();
        var result2 = strategy.SelectChromosomesForElimination(population, offspring, random2).ToList();

        // Assert
        // Both should eliminate the same number
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal(offspring.Length, result1.Count);
        
        // But likely different chromosomes (randomness test)
        // Note: There's a small chance they could be the same, but very unlikely with different seeds
        var areDifferent = !result1.SequenceEqual(result2);
        Assert.True(areDifferent || result1.Count <= 1); // Allow same result only if eliminating 1 or fewer
    }

    [Fact]
    public void ApplyReplacement_ShouldMaintainPopulationSize()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12])
        };

        var offspring = new[]
        {
            new DummyChromosome([13, 14, 15]),
            new DummyChromosome([16, 17, 18])
        };

        // Act
        var result = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        // Population size should be maintained: original size - eliminated + offspring = original size
        Assert.Equal(population.Length, result.Length);
        
        // All offspring should be present
        Assert.Contains(result, c => c.Genes.SequenceEqual(offspring[0].Genes));
        Assert.Contains(result, c => c.Genes.SequenceEqual(offspring[1].Genes));
        
        // Some original chromosomes should survive
        var survivingOriginals = result.Intersect(population).Count();
        Assert.Equal(population.Length - offspring.Length, survivingOriginals);
    }

    [Fact]
    public void ApplyReplacement_WithSingleOffspring_ShouldEliminateOneChromosome()
    {
        // Arrange
        var strategy = new RandomEliminationReplacementStrategy<int>();
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9])
        };

        var offspring = new[]
        {
            new DummyChromosome([10, 11, 12])
        };

        // Act
        var result = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        Assert.Equal(population.Length, result.Length); // Same size as original
        Assert.Contains(result, c => c.Genes.SequenceEqual(offspring[0].Genes)); // Offspring present
        
        var survivingOriginals = result.Intersect(population).Count();
        Assert.Equal(2, survivingOriginals); // 3 - 1 = 2 should survive
    }
}
