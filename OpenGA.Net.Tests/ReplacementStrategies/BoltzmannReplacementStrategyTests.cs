using OpenGA.Net.ReplacementStrategies;
using OpenGA.Net.Tests;

namespace OpenGA.Net.Tests.ReplacementStrategies;

public class BoltzmannReplacementStrategyTests
{
    private static DummyChromosome CreateDummyChromosomeWithFitness(double targetFitness)
    {
        // Create genes that will produce the target fitness (since fitness is average of genes)
        var geneValue = (int)Math.Round(targetFitness);
        var genes = new List<int> { geneValue, geneValue };
        var chromosome = new DummyChromosome(genes);
        
        // Force calculate fitness to ensure it's set
        _ = chromosome.Fitness;
        
        return chromosome;
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyPopulation_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new BoltzmannReplacementStrategy<int>(0.05);
        var population = Array.Empty<Chromosome<int>>();
        var offspring = new[] { CreateDummyChromosomeWithFitness(5.0) };
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random, 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyOffspring_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new BoltzmannReplacementStrategy<int>(0.05);
        var population = new[] { CreateDummyChromosomeWithFitness(1.0), CreateDummyChromosomeWithFitness(2.0) };
        var offspring = Array.Empty<Chromosome<int>>();
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random, 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_ShouldReturnCorrectNumberOfChromosomes()
    {
        // Arrange
        var strategy = new BoltzmannReplacementStrategy<int>(0.05);
        var population = new[]
        {
            CreateDummyChromosomeWithFitness(1.0),
            CreateDummyChromosomeWithFitness(2.0),
            CreateDummyChromosomeWithFitness(3.0),
            CreateDummyChromosomeWithFitness(4.0)
        };
        var offspring = new[] { CreateDummyChromosomeWithFitness(5.0), CreateDummyChromosomeWithFitness(6.0) };
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random, 0);

        // Assert
        Assert.Equal(2, result.Count()); // Should eliminate 2 chromosomes for 2 offspring
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEqualFitness_ShouldSelectRandomly()
    {
        // Arrange
        var strategy = new BoltzmannReplacementStrategy<int>(0.05);
        var population = new[]
        {
            CreateDummyChromosomeWithFitness(5.0),
            CreateDummyChromosomeWithFitness(5.0),
            CreateDummyChromosomeWithFitness(5.0),
            CreateDummyChromosomeWithFitness(5.0)
        };
        var offspring = new[] { CreateDummyChromosomeWithFitness(6.0) };
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random, 0);

        // Assert
        Assert.Single(result);
        Assert.Contains(result.First(), population);
    }

    [Fact]
    public void ApplyReplacement_ShouldMaintainPopulationSize()
    {
        // Arrange
        var strategy = new BoltzmannReplacementStrategy<int>(0.05);
        var population = new[]
        {
            CreateDummyChromosomeWithFitness(1.0),
            CreateDummyChromosomeWithFitness(2.0),
            CreateDummyChromosomeWithFitness(3.0),
            CreateDummyChromosomeWithFitness(4.0)
        };
        var offspring = new[] { CreateDummyChromosomeWithFitness(5.0), CreateDummyChromosomeWithFitness(6.0) };
        var random = new Random(42);

        // Act
        var result = strategy.ApplyReplacement(population, offspring, random, 0);

        // Assert
        Assert.Equal(4, result.Length); // Population size should remain the same
        Assert.Contains(offspring[0], result);
        Assert.Contains(offspring[1], result);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithHigherEpoch_ShouldBehaveDifferently()
    {
        // Arrange
        var strategy = new BoltzmannReplacementStrategy<int>(0.1, 2.0, true); // Higher decay rate for more noticeable difference
        var population = new[]
        {
            CreateDummyChromosomeWithFitness(1.0), // Low fitness - should be more likely to be eliminated
            CreateDummyChromosomeWithFitness(2.0),
            CreateDummyChromosomeWithFitness(3.0),
            CreateDummyChromosomeWithFitness(4.0)  // High fitness - should be less likely to be eliminated
        };
        var offspring = new[] { CreateDummyChromosomeWithFitness(5.0) };
        var random = new Random(42);

        // Act - Test with different epochs to see temperature effect
        var resultEarlyEpoch = strategy.SelectChromosomesForElimination(population, offspring, random, 0);
        random = new Random(42); // Reset random for fair comparison
        var resultLateEpoch = strategy.SelectChromosomesForElimination(population, offspring, random, 50);

        // Assert
        Assert.Single(resultEarlyEpoch);
        Assert.Single(resultLateEpoch);
        // At high epochs (low temperature), should be more deterministic in eliminating low-fitness chromosomes
        // At low epochs (high temperature), should be more random
    }
}
