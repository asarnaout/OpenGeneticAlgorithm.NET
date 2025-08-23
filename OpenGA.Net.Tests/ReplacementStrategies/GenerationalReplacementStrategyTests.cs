using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests.ReplacementStrategies;

public class GenerationalReplacementStrategyTests
{
    [Fact]
    public void Constructor_ShouldSucceed()
    {
        // Arrange & Act
        var strategy = new GenerationalReplacementStrategy<int>();

        // Assert - Should not throw any exceptions
        Assert.NotNull(strategy);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyPopulation_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = Array.Empty<Chromosome<int>>();
        var offspring = new[] { new DummyChromosome([1, 2, 3]) };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithPopulation_ShouldReturnEntirePopulation()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9])
        };
        var offspring = new[]
        {
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15])
        };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        // Should eliminate the entire population regardless of offspring count
        Assert.Equal(population.Length, result.Count());
        Assert.Equal(population.Length, result.Intersect(population).Count());
        
        // Verify all population chromosomes are selected for elimination
        foreach (var chromosome in population)
        {
            Assert.Contains(chromosome, result);
        }
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyOffspring_ShouldStillReturnEntirePopulation()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };
        var offspring = Array.Empty<Chromosome<int>>();

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        // Should eliminate entire population even with no offspring
        Assert.Equal(population.Length, result.Count());
        Assert.Equal(population.Length, result.Intersect(population).Count());
    }

    [Fact]
    public void ApplyReplacement_ShouldReplaceEntirePopulationWithOffspring()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6]),
            new DummyChromosome([7, 8, 9])
        };
        var offspring = new[]
        {
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15])
        };

        // Act
        var newPopulation = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        // New population should consist entirely of offspring
        Assert.Equal(offspring.Length, newPopulation.Length);
        Assert.Equal(offspring.Length, newPopulation.Intersect(offspring).Count());
        
        // No parent chromosomes should survive
        Assert.Empty(newPopulation.Intersect(population));
        
        // Verify all offspring are in the new population
        foreach (var offspringChromosome in offspring)
        {
            Assert.Contains(offspringChromosome, newPopulation);
        }
    }

    [Fact]
    public void ApplyReplacement_WithEmptyOffspring_ShouldReturnEmptyPopulation()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };
        var offspring = Array.Empty<Chromosome<int>>();

        // Act
        var newPopulation = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        // New population should be empty if no offspring
        Assert.Empty(newPopulation);
    }

    [Fact]
    public void ApplyReplacement_WithEmptyPopulation_ShouldReturnOnlyOffspring()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = Array.Empty<Chromosome<int>>();
        var offspring = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };

        // Act
        var newPopulation = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        // New population should consist entirely of offspring
        Assert.Equal(offspring.Length, newPopulation.Length);
        Assert.Equal(offspring.Length, newPopulation.Intersect(offspring).Count());
    }

    [Fact]
    public void ApplyReplacement_ShouldMaintainOffspringOrder()
    {
        // Arrange
        var strategy = new GenerationalReplacementStrategy<int>();
        var random = new Random(42);
        var population = new[]
        {
            new DummyChromosome([1, 2, 3])
        };
        var offspring = new[]
        {
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15]),
            new DummyChromosome([16, 17, 18])
        };

        // Act
        var newPopulation = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        // New population should maintain offspring order
        Assert.Equal(offspring.Length, newPopulation.Length);
        for (int i = 0; i < offspring.Length; i++)
        {
            Assert.Same(offspring[i], newPopulation[i]);
        }
    }
}
